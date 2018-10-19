using Diestel;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                string[] SkuValues = ExtSKU.SeparateSku(SKU);

                string _Prefix = SkuValues[0];
                if (_Prefix=="TN")
                {
                    List<Field> array = new List<Field>();

                    array.Add(new Field() { Name = "ID_GRP", Value = Group, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "ID_CHAIN", Value = Chain, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "ID_MERCHANT", Value = Merchant, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "ID_POS", Value = POS, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "DateTime", Value = "", Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "SKU", Value = SKU, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "PhoneNumber", Value = Reference, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "TransNumber", Value = 0, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "ID_Product", Value = "", Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "ID_COUNTRY", Value = 0, Length = 0, Type = 0, Class = 0, Encrypt = false });
                    array.Add(new Field() { Name = "TC", Value = 0, Length = 0, Type = 0, Class = 0, Encrypt = false });

                    var a = array;
                    return array;
                }

                if (_Prefix == "DT")
                {
                    string[] sp = ExtensionMethods.ExtSKU.SeparateSku(SKU);
                    string Prefix = sp[0];
                    string Sku = sp[1];

                    List<Field> lsFields = null;
                    var Config = new Dictionary<string, int>();
                    Config["Group"] = Group;
                    Config["Chain"] = Chain;
                    Config["Merchant"] = Merchant;
                    Config["POS"] = POS;
                    Config["Cashier"] = Cashier;

                    if (Prefix == "DT")
                    {
                        // Transacción: 1
                        RequestActiveService request = new RequestActiveService(Config, Sku, Reference, "1", User, Password, EncryptedKey);
                        var info = request.RequestService();
                        var response = info.response;

                        response = ReplaceFormat.ReplaceFrom(response, "ToListFields");

                        var root = JArray.Parse(response);
                        lsFields = JsonConvert.DeserializeObject<List<Field>>(root.ToString());
                    }
                }
                return new List<Field>();
            }


            public ResponseService Request(List<Field> Fields)
            {
                string _Prefix="";
                foreach (var item in Fields)
                {
                    if (item.Name=="SKU")
                    {  
                        string[] SkuValues = item.Value.ToString().Split("-");
                         _Prefix = SkuValues[0];
                        item.Value = SkuValues[1];
                    }
                }

                if (_Prefix=="TN")
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
                }

                if (!IsConfigured) throw new Exception();

                else if ( _Prefix=="DT")
                {
                    Dictionary<string, int> Config = new Dictionary<string, int>();

                    Config["Group"] = Group;
                    Config["Chain"] = Chain;
                    Config["Merchant"] = Merchant;
                    Config["POS"] = POS;
                    Config["Cashier"] = Cashier;


                    var request = new PayServiceDiestel(Config, User, Password, EncryptedKey);

                    string fields = JsonConvert.SerializeObject(Fields);
                    fields = ReplaceFormat.ReplaceFrom(fields, "ToListCampos");

                    var lsCampo = JsonConvert.DeserializeObject<List<cCampo>>(fields);

                    var x = request.PayService(lsCampo);

                    var responseService = new ResponseService();
                    responseService.Success = x.status;

                    if (responseService.Success)
                    {
                        responseService.XML = "N/A";
                        responseService.ResponseCode = 1001;
                    
                        string jsonCampos = x.response;
                        jsonCampos = ReplaceFormat.ReplaceFrom(jsonCampos, "ToListFields");

                        var newFields = JsonConvert.DeserializeObject<List<Field>>(jsonCampos);
                        responseService.Fields = newFields;

                        foreach (var item in newFields)
                        {
                            if (item.Name == "AUTORIZACION")
                            {
                                responseService.AuthorizeCode = int.Parse(item.Value.ToString());
                                break;
                            }
                        }
                    }
                    else if (!responseService.Success)
                    {

                    }

                    return responseService;
                }

                return new ResponseService();
            }

            public ResponseService Check(List<Field> Fields)
            {
                string _Prefix = "";
                foreach (var item in Fields)
                {
                    if (item.Name == "SKU")
                    {
                        string[] SkuValues = item.Value.ToString().Split("-");
                        _Prefix = SkuValues[0];
                        item.Value = SkuValues[1];
                    }
                }
                if (_Prefix == "TN")
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
                }

                if (_Prefix == "DT")
                {
                    if (!IsConfigured) throw new Exception();

                    string jsFields = JsonConvert.SerializeObject(Fields);
                    jsFields = ReplaceFormat.ReplaceFrom(jsFields, "ToListCampos");
                    var campos = JsonConvert.DeserializeObject<List<cCampo>>(jsFields);

                    var request = new CheckService(campos, User, Password, EncryptedKey);
                    var response = request.CheckServiceRequest();

                    var responseService = new ResponseService();
                    responseService.Success = response.status;
                    if (responseService.Success)
                    {
                        responseService.XML = "N/A";
                        responseService.ResponseCode = 1001;

                        string jsonCampos = response.Response;
                        jsonCampos = ReplaceFormat.ReplaceFrom(jsonCampos, "ToListFields");

                        var fields = JsonConvert.DeserializeObject<List<Field>>(jsonCampos);
                        responseService.Fields = fields;

                        foreach (var item in fields)
                        {
                            if (item.Name == "AUTORIZACION")
                            {
                                responseService.AuthorizeCode = int.Parse(item.Value.ToString());
                                break;
                            }
                        }
                    }

                    return responseService;
                }

                if (!IsConfigured) throw new Exception();

                return new ResponseService();

            }

            public ResponseService Cancel(List<Field> Fields)
            {

                if (!IsConfigured) throw new Exception();

                if (true/*"DT"*/)
                {
                    string fields = JsonConvert.SerializeObject(Fields);
                    fields = ReplaceFormat.ReplaceFrom(fields, "ToListCampos");
                    var lsCampos = JsonConvert.DeserializeObject<List<cCampo>>(fields);

                    List<Field> lsFields = null;

                    var Config = new Dictionary<string, int>();
                    Config["Group"] = Group;
                    Config["Chain"] = Chain;
                    Config["Merchant"] = Merchant;
                    Config["POS"] = POS;
                    Config["Cashier"] = Cashier;

                    var cancelation = new CancelationService(Config, User, Password, EncryptedKey, 1);
                    var request = cancelation.Reverses(lsCampos);

                    var x = request.result;
                    string jsonFields = ReplaceFormat.ReplaceFrom(x, "ToListFields");
                    lsFields = JsonConvert.DeserializeObject<List<Field>>(jsonFields);

                    var responseService = new ResponseService();
                    responseService.Success = request.status;

                    if (responseService.Success)
                    {
                        responseService.XML = "N/A";
                        responseService.Fields = lsFields;

                        foreach (var item in Fields)
                        {
                            if (item.Name == "AUTORIZACION")
                            {
                                responseService.AuthorizeCode = int.Parse(item.Value.ToString());
                                break;
                            }
                        }

                        foreach (var item in lsFields)
                        {
                            if (item.Name == "CODIGORESPUESTADESCR")
                            {
                                responseService.ResponseCode = int.Parse(item.Value.ToString());
                                break;
                            }
                        }
                    }
                    return responseService;
                }

                return new ResponseService();

            }
        }
    }

    //Tadenor
    public class Class_TN
    {
 
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
    //Diestel
    //Consulta del Servicio
    public class RequestActiveService
    {
        //Configuracion
        private Dictionary<string, int> Config = new Dictionary<string, int>();

        private string SKU;
        private string Reference;
        private long   currentTransaction;
        private string User;
        private string Password;
        private string EncryptionKey;

        public RequestActiveService(Dictionary<string, int> Config, string _Sku, string _Reference, string tx, string _User, string _Pwd, string _EKey)
        {
            //Datos de la configuracion
            this.Config["Group"] = Config["Group"];
            this.Config["Chain"] = Config["Chain"];
            this.Config["Merchant"] = Config["Merchant"];
            this.Config["POS"] = Config["POS"];
            this.Config["Cashier"] = Config["Cashier"];

            SKU = _Sku;
            Reference = _Reference;
            currentTransaction = long.Parse(tx);
            User = _User;
            Password = _Pwd;
            EncryptionKey = _EKey;
        }

        public (string response, bool status) RequestService()
        {
            PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);
            
            try
            {
                string jsonResult = "";

                cCampo[] requestInfo = new cCampo[10];

                cCampo[] response = null;

                requestInfo[0] = new cCampo();
                requestInfo[0].iTipo = eTipo.NE;
                requestInfo[0].sCampo = "IDGRUPO";
                requestInfo[0].sValor = Config["Group"];

                requestInfo[1] = new cCampo();
                requestInfo[1].iTipo = eTipo.NE;
                requestInfo[1].sCampo = "IDCADENA";
                requestInfo[1].sValor = Config["Chain"];

                requestInfo[2] = new cCampo();
                requestInfo[2].iTipo = eTipo.NE;
                requestInfo[2].sCampo = "IDTIENDA";
                requestInfo[2].sValor = Config["Merchant"];

                requestInfo[3] = new cCampo();
                requestInfo[3].iTipo = eTipo.NE;
                requestInfo[3].sCampo = "IDPOS";
                requestInfo[3].sValor = Config["POS"];

                requestInfo[4] = new cCampo();
                requestInfo[4].iTipo = eTipo.NE;
                requestInfo[4].sCampo = "IDCAJERO";
                requestInfo[4].sValor = Config["Cashier"];

                requestInfo[5] = new cCampo();
                requestInfo[5].iTipo = eTipo.FD;
                requestInfo[5].sCampo = "FECHALOCAL";
                requestInfo[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestInfo[6] = new cCampo();
                requestInfo[6].iTipo = eTipo.HR;
                requestInfo[6].sCampo = "HORALOCAL";
                requestInfo[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                requestInfo[7] = new cCampo();
                requestInfo[7].iTipo = eTipo.NE;
                requestInfo[7].sCampo = "TRANSACCION";
                requestInfo[7].sValor = currentTransaction;

                requestInfo[8] = new cCampo();
                requestInfo[8].iTipo = eTipo.AN;
                requestInfo[8].sCampo = "SKU";
                requestInfo[8].sValor = SKU;

                if (Reference != string.Empty)
                {
                    requestInfo[9] = new cCampo();
                    requestInfo[9].sCampo = "REFERENCIA";
                    requestInfo[9].sValor = Encriptacion.PXEncryptFX(Reference, EncryptionKey);
                    requestInfo[9].bEncriptado = true;
                    requestInfo[9].iTipo = eTipo.AN;
                }

                if (User == string.Empty || Password == string.Empty)
                {
                    string result = "Imposible conectar al WS porque no hay credenciales";
                    return (result, false);
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = User;
                    wservice.ClientCredentials.UserName.Password = Password;
                }

                

                try
                {
                    var task = Task.Run(() => wservice.InfoAsync(requestInfo));
                    
                    // Se establece el tiempo de espera para la respuesta
                    var timeout = TimeSpan.FromSeconds(45);

                    // Obtendremos: TRUE si la tarea se ejecuto dentro del tiempo establecido
                    //              FALSE si la tarea sobrepaso de ese tiempo
                    var isTaskFinished = task.Wait(timeout);

                    // Se va contabilizando los intentos para obtener respuesta del WS
                    int attempts = 1;
                    for (int i = 0; i < 3; i++)
                    {
                        //Si se obtuvo respuesta se detiene la iteracion y
                        // continuara con la logica establecida
                        if (isTaskFinished)
                        {
                            response = task.Result;
                            break;
                        }
                        else
                        {
                            //SE incremetnaran los intentos hasta obtener una respuesta
                            attempts++;

                            task = Task.Run(() => wservice.InfoAsync(requestInfo));
                        }
                    }

                    List<cCampo> lsCampos = null;

                    if (response.Length > 0)
                    {
                        int codeResponse;
                        foreach (var item in response)
                        {
                            if (item.sCampo == "CODIGORESPUESTA")
                            {
                                codeResponse = int.Parse(item.sValor.ToString());

                                //registrar el codigo de error en la bd

                                return (codeResponse.ToString(), false);
                            }

                            if (item.bEncriptado == true)
                            {
                                item.sValor = Encriptacion.PXDecryptFX(item.sValor.ToString(), EncryptionKey);
                            }

                            lsCampos.Add(item);
                        }
                        jsonResult = JsonConvert.SerializeObject(lsCampos);
                    }
                    return (jsonResult, true);
                }
                catch (System.Net.WebException wex)
                {
                    return (null, false);
                }
            }
            catch (System.Net.WebException wex)
            {
                return (null, false);
            }
        }
    }

    //Realizar el pago de servicio
    public class PayServiceDiestel
    {
        private Dictionary<string, int> Config = new Dictionary<string, int>();
        private string User;
        private string Password;
        private string EncryptedKey;
        private long CurrentTransaction;

        private string SKU;
        private string PaymentType;
        
        public PayServiceDiestel(Dictionary<string, int> Config, string User, string Password, string EncryptedKey, long currentTransaction = 1)
        {
            this.Config["Group"] = Config["Group"];
            this.Config["Chain"] = Config["Chain"];
            this.Config["Merchant"] = Config["Merchant"];
            this.Config["POS"] = Config["POS"];
            this.Config["Cashier"] = Config["Cashier"];

            this.User = User;
            this.Password = Password;
            this.EncryptedKey = EncryptedKey;
            this.CurrentTransaction = currentTransaction;
        }


        public (string response, string request, bool status) PayService(List<cCampo> campos)
        {
            foreach (var campo in campos)
            {
                int count = 0;
                if (campo.sCampo == "SKU")
                {
                    count = campos.IndexOf(campo);
                    SKU = campo.sValor.ToString();
                    campos.RemoveAt(count);
                    count = 0;
                }
                if (campo.sCampo == "TIPOPAGO")
                {
                    count = campos.IndexOf(campo);
                    PaymentType = campo.sValor.ToString();
                    campos.RemoveAt(count);
                    count = 0;
                }
            }

            // WS Diestel
            PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);

            // Cantidad de elementos dentro del arreglo
            int elements = campos.Count();

            // Cantidad de campos que tendra la solicitud del WS
            int index = 11 + elements;

            // Respuesta del pago procesado
            string jsonString = "";

            //Peticion del pago
            string jsRequest = "";

            //Campos para la solicitud
            cCampo[] requestEjecuta = new cCampo[index];

            try
            {
                requestEjecuta[0] = new cCampo();
                requestEjecuta[0].iTipo = eTipo.NE;
                requestEjecuta[0].sCampo = "IDGRUPO";
                requestEjecuta[0].sValor = Config["Group"];

                requestEjecuta[1] = new cCampo();
                requestEjecuta[1].iTipo = eTipo.NE;
                requestEjecuta[1].sCampo = "IDCADENA";
                requestEjecuta[1].sValor = Config["Chain"];

                requestEjecuta[2] = new cCampo();
                requestEjecuta[2].iTipo = eTipo.NE;
                requestEjecuta[2].sCampo = "IDTIENDA";
                requestEjecuta[2].sValor = Config["Merchant"];

                requestEjecuta[3] = new cCampo();
                requestEjecuta[3].iTipo = eTipo.NE;
                requestEjecuta[3].sCampo = "IDPOS";
                requestEjecuta[3].sValor = Config["POS"];

                requestEjecuta[4] = new cCampo();
                requestEjecuta[4].iTipo = eTipo.NE;
                requestEjecuta[4].sCampo = "IDCAJERO";
                requestEjecuta[4].sValor = Config["Cashier"];

                requestEjecuta[5] = new cCampo();
                requestEjecuta[5].iTipo = eTipo.FD;
                requestEjecuta[5].sCampo = "FECHALOCAL";
                requestEjecuta[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestEjecuta[6] = new cCampo();
                requestEjecuta[6].iTipo = eTipo.HR;
                requestEjecuta[6].sCampo = "HORALOCAL";
                requestEjecuta[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                requestEjecuta[7] = new cCampo();
                requestEjecuta[7].iTipo = eTipo.FD;
                requestEjecuta[7].sCampo = "FECHACONTABLE";
                requestEjecuta[7].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestEjecuta[8] = new cCampo();
                requestEjecuta[8].iTipo = eTipo.NE;
                requestEjecuta[8].sCampo = "TRANSACCION";
                requestEjecuta[8].sValor = CurrentTransaction;
                                
                requestEjecuta[9] = new cCampo();
                requestEjecuta[9].iTipo = eTipo.AN;
                requestEjecuta[9].sCampo = "SKU";
                requestEjecuta[9].sValor = SKU;

                requestEjecuta[10] = new cCampo();
                requestEjecuta[10].iTipo = eTipo.AN;
                requestEjecuta[10].sCampo = "TIPOPAGO";
                requestEjecuta[10].sValor = PaymentType;

                //Se recorre todo el arreglo "principal" (request)
                for (int i = 0; i < requestEjecuta.Length; i++)
                {
                    //Identificamos que posicion del arreglo es nula
                    //para posteriormente inicializarla con un valor
                    if (requestEjecuta[i] == null)
                    {
                        //Se declara una variable para indicar la poscicion
                        //del arreglo recibido como parametro del metodo (info)
                        int cp = 0;

                        //Recorremos e igualamos las posiciones
                        for (int j = i; j < requestEjecuta.Length; j++)
                        {
                            //Identificamos hasta que cantidad de elementos se
                            //deberan de añadir a los espacios nulos del arreglo principal
                            if (cp <= elements)
                            {
                                //Agregamos los nuevos elementos del arreglo recibido al
                                //arreglo principal
                                requestEjecuta[j] = new cCampo();
                                requestEjecuta[j].sCampo = campos[cp].sCampo;

                                //Se filtran los elemntos que deberan ir encriptados
                                if (campos[cp].sCampo == "REFERENCIA")
                                {
                                    requestEjecuta[j].sValor = Encriptacion.PXEncryptFX(campos[cp].sValor.ToString(), EncryptedKey);
                                    requestEjecuta[j].bEncriptado = true;
                                }
                                else
                                {
                                    requestEjecuta[j].sValor = campos[cp].sValor.ToString();
                                    requestEjecuta[j].bEncriptado = false;
                                }

                                //Se incrementa la variable para pasar al siguiente elemento que se debera
                                // de añadir al arreglo
                                cp++;
                            }
                        }
                    }
                }

                var listRequest = new List<cCampo>();
                foreach (var item in requestEjecuta)
                {
                    listRequest.Add(item);
                }

                jsRequest = JsonConvert.SerializeObject(listRequest);
                jsRequest = ReplaceFormat.ReplaceFrom(jsRequest, "ToListFields");

                //Se verifica que existan las credenciales
                if (User == string.Empty || Password == string.Empty || User == null || Password == null)
                {
                    jsonString = "Imposible conectar al WS porque no hay credenciales";
                    return (jsonString, jsRequest, false);
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = User;
                    wservice.ClientCredentials.UserName.Password = Password;
                }

                // Respuesta del WS
                cCampo[] response = null;

                try
                {

                    var task = Task.Run(() => wservice.EjecutaAsync(requestEjecuta));

                    // Se registra la solicitud de pago enviada en el TXT
                    //LogReversos.WriteTXData("+", "Solicitud de Pago enviada", currentTransaction.ToString());

                    // Se establece el tiempo de espera para la respuesta
                    var timeout = TimeSpan.FromSeconds(45);

                    // Obtendremos: TRUE si la tarea se ejecuto dentro del tiempo establecido
                    //              FALSE si la tarea sobrepaso de ese tiempo
                    var isTaskFinished = task.Wait(timeout);

                    // Se va contabilizando los intentos para obtener respuesta del WS
                    int attempts = 1;
                    for (int i = 0; i < 3; i++)
                    {
                        //Si se obtuvo respuesta se detiene la iteracion y
                        // continuara con la logica establecida
                        if (isTaskFinished)
                        {
                            response = task.Result;
                            break;
                        }
                        else
                        {
                            //SE incremetnaran los intentos hasta obtener una respuesta
                            attempts++;

                            //Actualizar tabla [Transaccion] con el intento

                            task = Task.Run(() => wservice.EjecutaAsync(requestEjecuta));
                        }
                    }

                    //Se recopilan los campos de la respuesta y se meten dentro de una lista
                    //en caso de requerir hacer una reversa
                    var listReverse = new List<cCampo>();
                    foreach (var campo in response)
                    {
                        listReverse.Add(campo);
                    }

                    //Verificams que obtengamos una respuesta
                    if (response.Length > 0)
                    {
                        int codeResponse;
                        foreach (var item in response)
                        {
                            if (item.sCampo == "CODIGORESPUESTA")
                            {
                                //Actualizar tabla transacciones
                                // el intento, CodeResponse, y el status false
                                codeResponse = int.Parse(item.sValor.ToString());

                                if (codeResponse == 47)
                                {
                                    //Se debera devolver un "ticket"
                                    //demostrando el motivo del rechazo
                                    //de la operacion
                                    //
                                    // Devolver un JSON a la Front-End
                                }
                                else if (codeResponse == 8 || codeResponse == 71 || codeResponse == 72)
                                {
                                    //Proceso de reversas
                                    CancelationService cancelation = new CancelationService(Config, User, Password, EncryptedKey);
                                    var cancel = cancelation.Reverses(listReverse);
                                    return (cancel.result, jsRequest, false);
                                }
                                return (codeResponse.ToString(), jsRequest, false);
                            }
                        }
                    }
                }
                catch (System.Net.WebException)
                {
                    //Proceso de reversas
                }

                //Lista con los campos que se le van a presentar 
                // a la front-end
                var listCampos = new List<cCampo>();
                foreach (var wsCampo in response)
                {
                    if (wsCampo.sCampo == "REFERENCIA")
                    {
                        wsCampo.sValor = Encriptacion.PXDecryptFX(wsCampo.sValor.ToString(), EncryptedKey);
                    }

                    listCampos.Add(wsCampo);
                }

                try
                {
                    //Proceso de insercion a la tabla Transacciones [Existoso]
                }
                catch (Exception) { throw; }

                //Serializa a json
                var jsonResult = JsonConvert.SerializeObject(listCampos);
                jsonString = jsonResult;
            }
            catch (Exception) { throw; }
            return (jsonString, jsRequest, true);
        }
    }

    //Realizar la cancelacion
    public class CancelationService
    {
        private Dictionary<string, int> Config;

        //Credenciales
        private string User;
        private string Password;
        private string EncryptedKey;

        //Datos requeridos
        private string SKU;           //[x]
        private string Reference;     //[x] _  Dentro del arreglo
        private int NoAuto;           //[x] _  cCampos
        private long currentTransaction;     //[x]
         

        public CancelationService(Dictionary<string, int> Config, string User, string Password, string EncryptedKey, long currentTransaction = 1)
        {
            this.Config = Config;
            this.User = User;
            this.Password = Password;
            this.EncryptedKey = EncryptedKey;
            this.currentTransaction = currentTransaction;
        }

        public (string result, bool status) Reverses(List<cCampo> campos)
        {
            foreach (var campo in campos)
            {
                if (campo.sCampo == "SKU")
                {
                    SKU = campo.sValor.ToString();
                }
                if (campo.sCampo == "REFERENCIA")
                {
                    Reference = campo.sValor.ToString();
                }
                if (campo.sCampo == "AUTORIZACION")
                {
                    NoAuto = int.Parse(campo.sValor.ToString());
                }
                break;
            }

            PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);

            cCampo[] requestReversa = new cCampo[12];

            string response = "";

            try
            {
                requestReversa[0] = new cCampo();
                requestReversa[0].iTipo = eTipo.NE;
                requestReversa[0].sCampo = "IDGRUPO";
                requestReversa[0].sValor = Config["Group"];

                requestReversa[1] = new cCampo();
                requestReversa[1].iTipo = eTipo.NE;
                requestReversa[1].sCampo = "IDCADENA";
                requestReversa[1].sValor = Config["Chain"];

                requestReversa[2] = new cCampo();
                requestReversa[2].iTipo = eTipo.NE;
                requestReversa[2].sCampo = "IDTIENDA";
                requestReversa[2].sValor = Config["Merchant"];

                requestReversa[3] = new cCampo();
                requestReversa[3].iTipo = eTipo.NE;
                requestReversa[3].sCampo = "IDPOS";
                requestReversa[3].sValor = Config["POS"];

                requestReversa[4] = new cCampo();
                requestReversa[4].iTipo = eTipo.NE;
                requestReversa[4].sCampo = "IDCAJERO";
                requestReversa[4].sValor = Config["Cashier"];

                requestReversa[5] = new cCampo();
                requestReversa[5].iTipo = eTipo.FD;
                requestReversa[5].sCampo = "FECHALOCAL";
                requestReversa[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestReversa[6] = new cCampo();
                requestReversa[6].iTipo = eTipo.HR;
                requestReversa[6].sCampo = "HORALOCAL";
                requestReversa[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                requestReversa[7] = new cCampo();
                requestReversa[7].iTipo = eTipo.NE;
                requestReversa[7].sCampo = "TRANSACCION";
                requestReversa[7].sValor = currentTransaction;

                requestReversa[8] = new cCampo();
                requestReversa[8].iTipo = eTipo.AN;
                requestReversa[8].sCampo = "SKU";
                requestReversa[8].sValor = SKU;

                requestReversa[9] = new cCampo();
                requestReversa[9].sCampo = "FECHACONTABLE";
                requestReversa[9].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestReversa[10] = new cCampo();
                requestReversa[10].sCampo = "AUTORIZACION";
                requestReversa[10].sValor = NoAuto;

                if (Reference != string.Empty)
                {
                    requestReversa[11] = new cCampo();
                    requestReversa[11].sCampo = "REFERENCIA";
                    requestReversa[11].sValor = Encriptacion.PXEncryptFX(Reference, EncryptedKey);
                    requestReversa[11].bEncriptado = true;
                    requestReversa[11].iTipo = eTipo.AN;
                }

                //Se verifica que existan las credenciales
                if (User == string.Empty || Password == string.Empty || User == null || Password == null)
                {
                    response = "Imposible conectar al WS porque no hay credenciales";
                    return (response, false);
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = User;
                    wservice.ClientCredentials.UserName.Password = Password;
                }

                cCampo[] fields = null;

                try
                {
                    var task = Task.Run(() => wservice.ReversaAsync(requestReversa));

                    // Se registra la solicitud de pago enviada en el TXT
                    //LogReversos.WriteTXData("+", "Solicitud de Pago enviada", currentTransaction.ToString());

                    // Se establece el tiempo de espera para la respuesta
                    var timeout = TimeSpan.FromSeconds(45);

                    // Obtendremos: TRUE si la tarea se ejecuto dentro del tiempo establecido
                    //              FALSE si la tarea sobrepaso de ese tiempo
                    var isTaskFinished = task.Wait(timeout);

                    // Se va contabilizando los intentos para obtener respuesta del WS
                    int attempts = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        //Si se obtuvo respuesta se detiene la iteracion y
                        // continuara con la logica establecida
                        if (isTaskFinished)
                        {
                            fields = task.Result;
                            break;
                        }
                        else
                        {
                            //SE incremetnaran los intentos hasta obtener una respuesta
                            attempts++;

                            //Actualizar tabla [Transaccion] con el intento

                            task = Task.Run(() => wservice.ReversaAsync(requestReversa));
                        }
                    }

                    if (fields[0].sCampo == "CODIGORESPUESTA" && fields[0].sValor.ToString() == "0")
                    {
                        //ACTUALIZAR en la BD de la reversa con los intentos
                        response = fields[0].sValor.ToString();
                        
                        return (response, true);
                    }
                    else
                    {
                        //Isertamos el resultado del WS en BD
                        response = fields[0].sValor.ToString();
                        return (response, true);
                    }
                }
                catch (System.Net.WebException wex)
                {
                    //Insertar por tiempo de espera BD
                    response = wex.ToString();
                    return (response, false);
                }

            }
            catch (Exception ex)
            {
                string respuesta = ex.ToString();
                return (respuesta, false);
            }
        }
    }

    //Metodo extendido para remplazar el formato json
    public static class ReplaceFormat
    {
        public static string ReplaceFrom(this string JsonFrom, string ToTypeList)
        {
            switch (ToTypeList)
            {
                case "ToListFields":
                    JsonFrom = JsonFrom.Replace("\"sCampo\":", "\"Name\":")
                                    .Replace("\"iTipo\":", "\"Type\":")
                                    .Replace("\"iLongitud\":", "\"Length\":")
                                    .Replace("\"iClase\":", "\"Class\":")
                                    .Replace("\"sValor\":", "\"Value\":")
                                    .Replace("\"bEncriptado\":", "\"Encrypt\":");
                    break;
                case "ToListCampos":
                    JsonFrom = JsonFrom.Replace("\"Name\":", "\"sCampo\":")
                           .Replace("\"Type\":", "\"iTipo\":")
                           .Replace("\"Length\":", "\"iLongitud\":")
                           .Replace("\"Class\":", "\"iClase\":")
                           .Replace("\"Value\":", "\"sValor\":")
                           .Replace("\"Encrypt\":", "\"bEncriptado\":");
                    break;
            }
            return JsonFrom;
        }
    }
    
    //Metodo extendido para Separar el SKU del Prefijo
    public static class ExtSKU
    {
        public static string[] SeparateSku(this string SKU)
        {
            string[] values = new string[2]; 

            string[] s = SKU.Split('-');
            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[0].Length == 2)
                {
                    values[0] = s[0];
                    if (s[1].Length == 13)
                    {
                        values[1] = s[1];
                        break;
                    }
                }
            }

            return values;
        }
    }

    public class CheckService
    {
        private string User;
        private string Password;
        private string EncryptedKey;

        private int Group;
        private int Chain;
        private int Merchant;
        private int POS;
        private int Cashier;
        private string LocalDate;
        private string LocalHour;
        private long Transaction;
        private string CountableDate;
        private string Sku;
        private string Reference;

        public CheckService(List<cCampo> campos, string User, string Password, string EncryptedKey)
        {
            this.User = User;
            this.Password = Password;
            this.EncryptedKey = EncryptedKey;

            foreach (var campo in campos)
            {
                switch (campo.sCampo)
                {
                    case "IDGRUPO":
                        Group = int.Parse(campo.sValor.ToString());
                        break;
                    case "IDCADENA":
                        Chain = int.Parse(campo.sValor.ToString());
                        break;
                    case "IDTIENDA":
                        Merchant = int.Parse(campo.sValor.ToString());
                        break;
                    case "IDPOS":
                        POS = int.Parse(campo.sValor.ToString());
                        break;
                    case "IDCAJERO":
                        Cashier = int.Parse(campo.sValor.ToString());
                        break;
                    case "FECHALOCAL":
                        LocalDate = (campo.sValor.ToString());
                        break;
                    case "HORALOCAL":
                        LocalHour = (campo.sValor.ToString());
                        break;
                    case "TRANSACCION":
                        Transaction = long.Parse(campo.sValor.ToString());
                        break;
                    case "FECHACONTABLE":
                        CountableDate = (campo.sValor.ToString());
                        break;
                    case "SKU":
                        Sku = (campo.sValor.ToString());
                        break;
                    case "REFERENCIA":
                        Reference = (campo.sValor.ToString());
                        break;
                }
            }
        }

        public (string Response, bool status) CheckServiceRequest()
        {
            PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);
            try
            {
                string jsonResult = "";

                cCampo[] response = null;

                cCampo[] requestCheck = new cCampo[10];

                requestCheck[0] = new cCampo();
                requestCheck[0].iTipo = eTipo.NE;
                requestCheck[0].sCampo = "IDGRUPO";
                requestCheck[0].sValor = Group;

                requestCheck[1] = new cCampo();
                requestCheck[1].iTipo = eTipo.NE;
                requestCheck[1].sCampo = "IDCADENA";
                requestCheck[1].sValor = Chain;

                requestCheck[2] = new cCampo();
                requestCheck[2].iTipo = eTipo.NE;
                requestCheck[2].sCampo = "IDTIENDA";
                requestCheck[2].sValor = Merchant;

                requestCheck[3] = new cCampo();
                requestCheck[3].iTipo = eTipo.NE;
                requestCheck[3].sCampo = "IDPOS";
                requestCheck[3].sValor = POS;

                requestCheck[4] = new cCampo();
                requestCheck[4].iTipo = eTipo.NE;
                requestCheck[4].sCampo = "IDCAJERO";
                requestCheck[4].sValor = Cashier;

                requestCheck[5] = new cCampo();
                requestCheck[5].iTipo = eTipo.FD;
                requestCheck[5].sCampo = "FECHALOCAL";
                requestCheck[5].sValor = LocalDate;

                requestCheck[6] = new cCampo();
                requestCheck[6].iTipo = eTipo.HR;
                requestCheck[6].sCampo = "HORALOCAL";
                requestCheck[6].sValor = LocalHour;

                requestCheck[7] = new cCampo();
                requestCheck[7].iTipo = eTipo.NE;
                requestCheck[7].sCampo = "TRANSACCION";
                requestCheck[7].sValor = Transaction;

                requestCheck[8] = new cCampo();
                requestCheck[8].iTipo = eTipo.AN;
                requestCheck[8].sCampo = "FECHACONTABLE";
                requestCheck[8].sValor = CountableDate;

                requestCheck[9] = new cCampo();
                requestCheck[9].iTipo = eTipo.AN;
                requestCheck[9].sCampo = "SKU";
                requestCheck[9].sValor = Sku;

                if (Reference != string.Empty)
                {
                    requestCheck[10] = new cCampo();
                    requestCheck[10].sCampo = "REFERENCIA";
                    requestCheck[10].sValor = Encriptacion.PXEncryptFX(Reference, EncryptedKey);
                    requestCheck[10].bEncriptado = true;
                    requestCheck[10].iTipo = eTipo.AN;
                }

                if (User == string.Empty || Password == string.Empty || User == null || Password == null)
                {
                    string result = "Imposible conectar al WS porque no hay credenciales";
                    return (result, false);
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = User;
                    wservice.ClientCredentials.UserName.Password = Password;
                }

                try
                {
                    var task = Task.Run(() => wservice.InfoAsync(requestCheck));

                    var timeout = TimeSpan.FromSeconds(45);

                    var isTaskFinished = task.Wait(timeout);

                    int attempts = 1;
                    for (int i = 0; i < 3; i++)
                    {
                        if (isTaskFinished)
                        {
                            response = task.Result;
                            break;
                        }
                        else
                        {
                            attempts++;
                            task = Task.Run(() => wservice.InfoAsync(requestCheck));
                        }
                    }

                    List<cCampo> lsCampos = null;

                    if (response.Length > 0)
                    {
                        int codeResponse;
                        foreach (var item in response)
                        {
                            if (item.sCampo == "CODIGORESPUESTA")
                            {
                                codeResponse = int.Parse(item.sValor.ToString());
                                return (codeResponse.ToString(), false);
                            }
                            lsCampos.Add(item);
                        }
                        jsonResult = JsonConvert.SerializeObject(lsCampos);
                    }
                    return (jsonResult, true);
                }
                catch (Exception) { throw; }
            }
            catch (Exception) { throw; }
        }
    }

    public class Encriptacion
    {
        public static string PXEncryptFX(string sInput, string sKey)
        {
            var inputLength = sInput.Length;
            var keyLength = sKey.Length;
            var keyValueCharArray = new int[keyLength];
            var inputValueCharArray = new int[inputLength];


            var num6 = 0; //Verificar nombre
            for (int i = 0; i < keyLength; i++)
            {
                int charCode = Strings.AscW(sKey.Substring(i, 1));
                keyValueCharArray[i] = charCode;
                num6 += (charCode * (i + 1)) % 9; //Verificar fomula
            }


            sInput = Reverse(sInput);
            var num11 = 0; //Igual que el 6 
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = Strings.AscW(sInput.Substring(i, 1));
                inputValueCharArray[i] = charCode;
                num11 += (charCode * (i + 1)) % 9; //Igual que el 6
            }


            var Number = (num11 + num6) % 143; //??
            var suffixValue = Number.ToString("X"); //Conversion.Hex()
            if (suffixValue.Length == 1) suffixValue = "0" + suffixValue;
            var num12 = (Strings.AscW(sKey.Substring(0, 1)) + Strings.AscW(sKey.Substring(keyLength - 1, 1)) + keyLength) % 9; //Verificar nombre
            if (num12 == 0) num12 = 20;

            //Primera encriptacion en base a codigo1 y posiciondelKey
            var num13 = (num6 + Number) % keyLength;
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = inputValueCharArray[i] + num12 + keyValueCharArray[num13];
                inputValueCharArray[i] = charCode <= 254 ? charCode : charCode - 254;
                if (num13 < (keyLength - 1)) ++num13; else num13 = 0;
            }

            //Segunda encriptacion en base a secretNumber
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = inputValueCharArray[i] + Number;
                inputValueCharArray[i] = charCode <= 254 ? charCode : charCode - 254;
            }

            //Nuevos codigos a hexadecimal
            string encodeString = "";
            for (int i = 0; i < inputLength; i++)
            {
                string hexValue = inputValueCharArray[i].ToString("X");
                if (hexValue.Length == 1)
                    hexValue = "0" + hexValue;
                encodeString += hexValue;
            }
            return Reverse(suffixValue + encodeString);
        }

        private static string Reverse(string v)
        {
            char[] chars = v.ToArray();
            string result = "";

            for (int i = 0, j = v.Length - 1; i < v.Length; i++, j--)
            {
                result += chars[j];
            }

            return result;
        }

        public static string PXDecryptFX(string sInput, string sKey)
        {
            var inputLength = (sInput.Length / 2) - 1;
            var keyLength = sKey.Length;
            var keyValueCharArray = new int[keyLength];
            var inputValueCharArray = new int[inputLength];

            var num6 = 0;
            for (int i = 0; i < keyLength; i++)
            {
                int charCode = Strings.AscW(sKey.Substring(i, 1));
                keyValueCharArray[i] = charCode;
                num6 += (charCode * (i + 1)) % 9;
            }

            var num8 = (Strings.AscW(sKey.Substring(0, 1)) + Strings.AscW(sKey.Substring(keyLength - 1, 1)) + keyLength) % 9;
            if (num8 == 0) num8 = 20;

            sInput = Reverse(sInput);
            var suffixCode = int.Parse(sInput.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            sInput = sInput.Substring(2, sInput.Length - 2);

            for (int i = 0; i < inputLength; i++)
            {
                int charCode = int.Parse(sInput.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                inputValueCharArray[i] = charCode;
            }

            var num14 = (num6 + suffixCode) % keyLength;
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = checked((int)(inputValueCharArray[i] - num8 - keyValueCharArray[num14]));
                inputValueCharArray[i] = charCode >= 1 ? charCode : 254 + charCode;
                if (num14 < keyLength - 1) ++num14; else num14 = 0;
            }


            for (int i = 0; i < inputLength; i++)
            {
                int charCode = inputValueCharArray[i] - suffixCode;
                inputValueCharArray[i] = charCode >= 1 ? charCode : 254 + charCode;
            }

            string decodeString = "";
            for (int i = 0; i < inputLength; i++)
            {
                decodeString += Strings.ChrW(inputValueCharArray[i]);
            }
            return Reverse(decodeString);
        }
    }
}