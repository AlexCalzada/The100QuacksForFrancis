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
using System.Text.RegularExpressions;
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
            public List<Field> Fields { get; set; }
            public string XML { get; set; }
            public bool Success { get; set; }
            public int ResponseCode { get; set; }
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

                string[] credentials = { Url, User, Password, EncryptedKey };
                int[] config = { Group, Chain, Merchant, POS, Cashier };
                List<Field> fields = new List<Field>
                {
                    new Field() { Name = "TRANSACCION", Value = 1, Length = 0, Type = 1, Class = 0, Encrypt = false },
                    new Field() { Name = "SKU", Value = SKU, Length = 0, Type = 0, Class = 0, Encrypt = false },
                    new Field() { Name = "REFERENCIA", Value = Reference, Length = 0, Type = 0, Class = 0, Encrypt = true }
                };
                var _DT = new Class_DT();
                var task = Task.Run(() => { return _DT.Send_Request("Info", credentials, fields, config); });
                int attempts = 1;
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var success = task.Wait(45000);
                        if (success)
                        {
                            var x = _DT.GetFieldsFromXml(task.Result.response);

                            // Datos que se deben volver a enviar al WS
                            // [SKU] [Referencia] [TipoPago] [Transaccion]
                            var persistData = _DT.persistData;
                            
                            var responseService = new ResponseService();


                            bool referenceExists = false;
                            bool skuExists = false;

                            if (x.field.Count > 0)
                            {
                                foreach (var item in x.field)
                                {
                                    if (item.Name == "CODIGORESPUESTA")
                                    {
                                        int[] errorCodes = { 13, 14, 18, 20, 21, 36 };
                                        for (int e = 0; e < errorCodes.Length; e++)
                                        {
                                            if (errorCodes[e] == int.Parse(item.Value.ToString()))
                                            {
                                                throw new ReferenceException(errorCodes[e].ToString());
                                            }
                                        }
                                        throw new ErrorCodeException(item.Value.ToString());
                                    }
                                    if (item.Encrypt == true)
                                    {
                                        item.Value = Class_DT.Decrypt(item.Value.ToString(), EncryptedKey);
                                        if (item.Name == "REFERENCIA")
                                        {
                                            referenceExists = x.field.Contains(item);
                                        }
                                    }
                                    if (item.Name == "SKU")
                                    {
                                        skuExists = x.field.Contains(item);
                                    }
                                }

                                if (referenceExists == true && skuExists == false)
                                {
                                    x.field.Add(new Field() { Name = "SKU", Value = persistData[0], Length = 13, Type = 0, Class = 0, Encrypt = false });
                                    x.field.Add(new Field() { Name = "TRANSACCION", Value = persistData[2], Length = 8, Type = 0, Class = 1, Encrypt = false });
                                    x.field.Add(new Field() { Name = "TIPOPAGO", Value = persistData[3], Length = 4, Type = 0, Class = 0, Encrypt = true });
                                }
                                else if(referenceExists == false && skuExists == true)
                                {
                                    x.field.Add(new Field() { Name = "TRANSACCION", Value = persistData[2], Length = 8, Type = 1, Class = 0, Encrypt = false });
                                    x.field.Add(new Field() { Name = "REFERENCIA", Value = persistData[1], Length = 30, Type = 0, Class = 0, Encrypt = true });
                                    x.field.Add(new Field() { Name = "TIPOPAGO", Value = persistData[3], Length = 4, Type = 0, Class = 0, Encrypt = true });
                                }
                                else if (referenceExists == false && skuExists == false)
                                {
                                    x.field.Add(new Field() { Name = "SKU", Value = persistData[0], Length = 13, Type = 0, Class = 0, Encrypt = false });
                                    x.field.Add(new Field() { Name = "TRANSACCION", Value = persistData[2], Length = 8, Type = 1, Class = 1, Encrypt = false });
                                    x.field.Add(new Field() { Name = "REFERENCIA", Value = persistData[1], Length = 30, Type = 0, Class = 0, Encrypt = true });
                                    x.field.Add(new Field() { Name = "TIPOPAGO", Value = persistData[3], Length = 4, Type = 0, Class = 0, Encrypt = true });
                                }

                                return x.field;
                            }
                        }
                        else
                        {
                            attempts++;

                            if (attempts == 3)
                            {
                                return new List<Field>();
                            }

                            task = Task.Run(() => { return _DT.Send_Request("Info", credentials, fields, config); });
                        }
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }
                }

                return new List<Field>();
            }

            public ResponseService Request(List<Field> Fields)
            {
                if (!IsConfigured) throw new Exception();
                string[] credentials = { Url, User, Password, EncryptedKey };
                int[] config = { Group, Chain, Merchant, POS, Cashier };
                var _DT = new Class_DT();
                var task = Task.Run(() => { return _DT.Send_Request("Ejecuta", credentials, Fields, config); });
                int attempts = 0;

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var success = task.Wait(45000);
                        if (success)
                        {
                            var x = _DT.GetFieldsFromXml(task.Result.response);
                            var responseService = new ResponseService();

                            if (x.field.Count > 0)
                            {
                                responseService.Success = true;
                                responseService.XML = task.Result.request;
                                responseService.ResponseCode = 0;

                                if (attempts >= 3)
                                {
                                   return Cancel(x.field);
                                }

                                foreach (var item in x.field)
                                {
                                    switch (item.Name)
                                    {
                                        case "CODIGORESPUESTA":
                                            int[] errorCodes = { 13, 14, 18, 20, 21, 36 };
                                            for (int e = 0; e < errorCodes.Length; e++)
                                            {
                                                if (errorCodes[e] == int.Parse(item.Value.ToString()))
                                                {
                                                    throw new ReferenceException(errorCodes[e].ToString());
                                                }
                                            }
                                            throw new ErrorCodeException(item.Value.ToString());
                                            //throw new Exception();
                                            //responseService.ResponseCode = int.Parse(item.Value.ToString());

                                            //try
                                            //{
                                            //    if (responseService.ResponseCode == 8 || responseService.ResponseCode == 71 || responseService.ResponseCode == 72)
                                            //    {
                                            //        responseService.AuthorizeCode = 0;
                                            //        responseService.Success = false;
                                            //        break;
                                            //    }
                                            //}
                                            //catch (Exception ex)
                                            //{
                                            //    throw new Exception($"{ex}");
                                            //}
                                            //break;
                                        case "AUTORIZACION":
                                            responseService.AuthorizeCode = int.Parse(item.Value.ToString());
                                            break;
                                        case "REFERENCIA":
                                            item.Value = Class_DT.Decrypt(item.Value.ToString(), EncryptedKey);
                                            break;
                                    }
                                }

                                responseService.Fields = x.field;
                                return responseService;
                            }
                        }
                        else
                        {
                            attempts++;

                            if (attempts == 3)
                            {
                                var responseService = new ResponseService();

                                responseService.AuthorizeCode = 0;
                                responseService.Fields = null;
                                responseService.ResponseCode = 8;
                                responseService.Success = false;
                                responseService.XML = task.Result.request;

                                return responseService;
                            }

                            task = Task.Run(() => { return _DT.Send_Request("Ejecuta", credentials, Fields, config); });
                        }
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }
                }
                return new ResponseService();
            }

            public ResponseService Check(List<Field> Fields)
            {
                if (!IsConfigured) throw new Exception();
                return new ResponseService();
            }

            public ResponseService Cancel(List<Field> Fields)
            {
                if (!IsConfigured) throw new Exception();

                string[] credentials = { Url, User, Password, EncryptedKey };
                int[] config = { Group, Chain, Merchant, POS, Cashier };

                var _DT = new Class_DT();

                var task = Task.Run(() => { return _DT.Send_Request("Reversa", credentials, Fields, config); });

                int attempts = 1;

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var success = task.Wait(45000);
                        if (success)
                        {
                            var x = _DT.GetFieldsFromXml(task.Result.response);
                            var responseService = new ResponseService();

                            if (x.field.Count > 0)
                            {
                                responseService.Success = true;
                                responseService.XML = task.Result.request;
                                responseService.AuthorizeCode = 0;

                                foreach (var item in x.field)
                                {
                                    switch (item.Name)
                                    {
                                        case "CODIGORESPUESTA":
                                            responseService.ResponseCode = int.Parse(item.Value.ToString());
                                            if (responseService.ResponseCode == 8 || responseService.ResponseCode == 71 || responseService.ResponseCode == 72)
                                            {
                                                responseService.Success = false;
                                            }
                                            break;
                                    }
                                }
                                responseService.Fields = x.field;
                                return responseService;
                            }
                        }
                        else
                        {
                            attempts++;

                            if (attempts == 3)
                            {
                                var responseService = new ResponseService();

                                responseService.AuthorizeCode = 0;
                                responseService.Fields = null;
                                responseService.ResponseCode = 8;
                                responseService.Success = false;
                                responseService.XML = task.Result.request;

                                return responseService;
                            }
                            task = Task.Run(() => { return _DT.Send_Request("Reversa", credentials, Fields, config); });
                        }
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }
                }
                return new ResponseService();
            }
        }
    }

    public class ReferenceException : Exception
    {
        public ReferenceException() { }
        public ReferenceException(string str)
        : base(String.Format("Error con la referencia: {0}", str)) { }
    }

    public class ErrorCodeException : Exception
    {
        public ErrorCodeException() { }
        public ErrorCodeException(string str)
        : base(String.Format("Error de servicio: {0}", str)) { }
    }

    //Diestel
    public class Class_DT
    {
        public object[] persistData { get; set; }

        public (string response, string request) Send_Request(string service, string[] credentials, List<Field> Fields, int[] config = null)
        {
            string sXML = "";

            HttpWebRequest webRequest = CreateWebRequest(credentials[0], "http://www.pagoexpress.com.mx/pxUniversal/" + service, credentials[1], credentials[2]);

            switch (service)
            {
                case "Info":
                    sXML = BuildXml(service, config, Fields, credentials[3]);
                    break;
                case "Ejecuta":
                    sXML = BuildXml(service, config, Fields, credentials[3]);
                    break;
                case "Reversa":
                    sXML = BuildXml(service, config, Fields, credentials[3]);
                    break;
            }

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

            //Modificar el Xml

            return (soapResult, sXML);
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

        public string BuildXml(string service, int[] config, List<Field> Fields, string EncryptedKey = "")
        {
            string SKU = "";
            long Transaction = 0;
            string PaymentType = "";
            string Reference = "";
            long Authorization = 0;

            for (int i = Fields.Count - 1; i >= 0; i--)
            {
                if (Fields[i].Name == "SKU" || Fields[i].Name == "TIPOPAGO" || Fields[i].Name == "TRANSACCION" || Fields[i].Name == "REFERENCIA" || Fields[i].Name == "AUTORIZACION")
                {
                    switch (Fields[i].Name)
                    {
                        case "SKU":
                            SKU = Fields[i].Value.ToString();
                            break;
                        case "REFERENCIA":
                            if (Reference == "" || Reference == null)
                            {
                                Reference = Fields[i].Value.ToString();
                                Reference = Encrypt(Reference, EncryptedKey);
                            }
                            break;
                        case "TIPOPAGO":
                            PaymentType = Fields[i].Value.ToString();
                            break;
                        case "TRANSACCION":
                            Transaction = long.Parse(Fields[i].Value.ToString());
                            break;
                    }
                    Fields.RemoveAt(i);
                }
            }

            string partXml = "";

            if (service == "Info")
            {
                partXml = "<cCampo>" +
                            "<sCampo>IDGRUPO</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[0] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDCADENA</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[1] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDTIENDA</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[2] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDPOS</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[3] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDCAJERO</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[4] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>FECHALOCAL</sCampo>" +
                            "<iTipo>FD</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("dd/MM/yyyy") + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>HORALOCAL</sCampo>" +
                            "<iTipo>HR</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("HH:mm:ss") + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>FECHACONTABLE</sCampo>" +
                            "<iTipo>FD</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("dd/MM/yyyy") + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>TRANSACCION</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + Transaction + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>SKU</sCampo>" +
                            "<iTipo>AN</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + SKU + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>REFERENCIA</sCampo>" +
                            "<iTipo>AN</iTipo>" +
                            "<iLongitud>120</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + Reference + "</sValor>" +
                            "<bEncriptado>true</bEncriptado>" +
                          "</cCampo>";
            }
            else if (service == "Ejecuta")
            {
                partXml = "<cCampo>" +
                           "<sCampo>IDGRUPO</sCampo>" +
                           "<iTipo>NE</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:int"">" + config[0] + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>IDCADENA</sCampo>" +
                           "<iTipo>NE</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:int"">" + config[1] + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>IDTIENDA</sCampo>" +
                           "<iTipo>NE</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:int"">" + config[2] + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>IDPOS</sCampo>" +
                           "<iTipo>NE</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:int"">" + config[3] + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>IDCAJERO</sCampo>" +
                           "<iTipo>NE</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:int"">" + config[4] + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>FECHALOCAL</sCampo>" +
                           "<iTipo>FD</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("dd/MM/yyyy") + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>HORALOCAL</sCampo>" +
                           "<iTipo>HR</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("HH:mm:ss") + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>FECHACONTABLE</sCampo>" +
                           "<iTipo>FD</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("dd/MM/yyyy") + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>TRANSACCION</sCampo>" +
                           "<iTipo>NE</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:int"">" + Transaction + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>SKU</sCampo>" +
                           "<iTipo>AN</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:string"">" + SKU + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         "<cCampo>" +
                         "<sCampo>TIPOPAGO</sCampo>" +
                           "<iTipo>AN</iTipo>" +
                           "<iLongitud>0</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:string"">" + PaymentType + "</sValor>" +
                           "<bEncriptado>false</bEncriptado>" +
                         "</cCampo>" +
                         " <cCampo>" +
                           "<sCampo>" + "REFERENCIA" + "</sCampo>" +
                           "<iTipo>AN</iTipo>" +
                           "<iLongitud>120</iLongitud>" +
                           "<iClase>0</iClase>" +
                           @"<sValor xsi:type=""xsd:string"">" + Reference + "</sValor>" +
                           "<bEncriptado>true</bEncriptado>" +
                         "</cCampo>";

                foreach (var field in Fields)
                {
                    string type = "0";
                    type = GetEnumValue(null, field.Type);

                    switch (type)
                    {
                        case "AN":
                            partXml += "<cCampo>" +
                                    "<sCampo>" + field.Name + "</sCampo>" +
                                    "<iTipo>" + type + "</iTipo>" +
                                    "<iLongitud>" + field.Length + "</iLongitud>" +
                                    "<iClase>" + field.Class + "</iClase>" +
                                    @"<sValor xsi:type=""xsd:string"">" + field.Value + "</sValor>" +
                                    "<bEncriptado>false</bEncriptado>" +
                                  "</cCampo>";
                            break;
                        case "NE":
                            partXml += "<cCampo>" +
                                    "<sCampo>" + field.Name + "</sCampo>" +
                                    "<iTipo>" + type + "</iTipo>" +
                                    "<iLongitud>" + field.Length + "</iLongitud>" +
                                    "<iClase>" + field.Class + "</iClase>" +
                                    @"<sValor xsi:type=""xsd:int"">" + field.Value + "</sValor>" +
                                    "<bEncriptado>false</bEncriptado>" +
                                  "</cCampo>";
                            break;
                        case "NM":
                            partXml += "<cCampo>" +
                                    "<sCampo>" + field.Name + "</sCampo>" +
                                    "<iTipo>" + type + "</iTipo>" +
                                    "<iLongitud>" + field.Length + "</iLongitud>" +
                                    "<iClase>" + field.Class + "</iClase>" +
                                    @"<sValor xsi:type=""xsd:decimal"">" + field.Value + "</sValor>" +
                                    "<bEncriptado>false</bEncriptado>" +
                                  "</cCampo>";
                            break;
                        case "FD":
                            partXml += "<cCampo>" +
                                    "<sCampo>" + field.Name + "</sCampo>" +
                                    "<iTipo>" + type + "</iTipo>" +
                                    "<iLongitud>" + field.Length + "</iLongitud>" +
                                    "<iClase>" + field.Class + "</iClase>" +
                                    @"<sValor xsi:type=""xsd:string"">" + field.Value + "</sValor>" +
                                    "<bEncriptado>false</bEncriptado>" +
                                  "</cCampo>";
                            break;
                        case "HR":
                            partXml += "<cCampo>" +
                                    "<sCampo>" + field.Name + "</sCampo>" +
                                    "<iTipo>" + type + "</iTipo>" +
                                    "<iLongitud>" + field.Length + "</iLongitud>" +
                                    "<iClase>" + field.Class + "</iClase>" +
                                    @"<sValor xsi:type=""xsd:string"">" + field.Value + "</sValor>" +
                                    "<bEncriptado>false</bEncriptado>" +
                                  "</cCampo>";
                            break;
                        case "ND":
                            partXml += "<cCampo>" +
                                    "<sCampo>" + field.Name + "</sCampo>" +
                                    "<iTipo>" + type + "</iTipo>" +
                                    "<iLongitud>" + field.Length + "</iLongitud>" +
                                    "<iClase>" + field.Class + "</iClase>" +
                                    @"<sValor xsi:type=""xsd:decimal"">" + field.Value + "</sValor>" +
                                    "<bEncriptado>false</bEncriptado>" +
                                  "</cCampo>";
                            break;
                    }
                }
            }
            else if (service == "Reversa")
            {
                partXml = "<cCampo>" +
                            "<sCampo>IDGRUPO</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[0] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDCADENA</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[1] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDTIENDA</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[2] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDPOS</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[3] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>IDCAJERO</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + config[4] + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>FECHALOCAL</sCampo>" +
                            "<iTipo>FD</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("dd/MM/yyyy") + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>HORALOCAL</sCampo>" +
                            "<iTipo>HR</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("HH:mm:ss") + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>FECHACONTABLE</sCampo>" +
                            "<iTipo>FD</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + DateTime.Now.ToString("dd/MM/yyyy") + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>TRANSACCION</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:int"">" + Transaction + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>SKU</sCampo>" +
                            "<iTipo>AN</iTipo>" +
                            "<iLongitud>0</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + SKU + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>REFERENCIA</sCampo>" +
                            "<iTipo>AN</iTipo>" +
                            "<iLongitud>120</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + Reference + "</sValor>" +
                            "<bEncriptado>true</bEncriptado>" +
                          "</cCampo>" +
                          "<cCampo>" +
                          "<sCampo>AUTORIZACION</sCampo>" +
                            "<iTipo>NE</iTipo>" +
                            "<iLongitud>20</iLongitud>" +
                            "<iClase>0</iClase>" +
                            @"<sValor xsi:type=""xsd:string"">" + Authorization + "</sValor>" +
                            "<bEncriptado>false</bEncriptado>" +
                          "</cCampo>";
            }

            //Se capturan los datos de la consulta: SKU, Referencia, Transaccion[NO], Tipo de Pago
            object[] persistData = { SKU, Decrypt(Reference, EncryptedKey), Transaction, "EFE" };
            this.persistData = persistData;

            string fullXml = "<cArrayCampos>" + partXml + "</cArrayCampos>";
            return fullXml;
        }

        public (List<Field> field, string xmlResponse) GetFieldsFromXml(string sXml)
        {
            string xmlResponse = sXml;

            sXml = RemoveAllNamespaces(sXml);

            sXml = Regex.Replace(sXml, @"\<(\/?)cCampo\>", m => m.Value.Contains("/") ? "</Field>" : "<Field>");
            sXml = Regex.Replace(sXml, @"\<(\/?)sCampo\>", m => m.Value.Contains("/") ? "</Name>" : "<Name>");
            sXml = Regex.Replace(sXml, @"\<(\/?)iTipo\>", m => m.Value.Contains("/") ? "</Type>" : "<Type>");
            sXml = Regex.Replace(sXml, @"\<(\/?)iLongitud\>", m => m.Value.Contains("/") ? "</Length>" : "<Length>");
            sXml = Regex.Replace(sXml, @"\<(\/?)iClase\>", m => m.Value.Contains("/") ? "</Class>" : "<Class>");
            sXml = Regex.Replace(sXml, @"\<(\/?)bEncriptado\>", m => m.Value.Contains("/") ? "</Encrypt>" : "<Encrypt>");

            string xmlDoc = "";
            XDocument Doc = XDocument.Parse(sXml);
            var items = Doc.Descendants("sValor");
            foreach (var item in items.ToList())
            {
                var value = (object)item;
                var newNode = new XElement("Value", value);
                item.ReplaceWith(newNode);
            }

            xmlDoc = Doc.ToString();
            string result = xmlDoc;

            XDocument document = XDocument.Parse(result);

            var listFields = (from s in document.Descendants("Field")
                        select new Field()
                        {
                            Name = s.Element("Name").Value.ToString(),
                            Type = int.Parse(GetEnumValue(s.Element("Type").Value.ToString(), -1)),
                            Length = Convert.ToInt32(s.Element("Length").Value.ToString()),
                            Class = Convert.ToInt32(s.Element("Class").Value.ToString()),
                            Value = s.Element("Value").Value,
                            Encrypt = Convert.ToBoolean(s.Element("Encrypt").Value.Equals("true"))
                        }).ToList();

            return (listFields, sXml);
        }

        public string ReplaceFrom(string JsonFrom)
        {
            JsonFrom = JsonFrom.Replace("\"sCampo\":", "\"Name\":")
                            .Replace("\"iTipo\":", "\"Type\":")
                            .Replace("\"iLongitud\":", "\"Length\":")
                            .Replace("\"iClase\":", "\"Class\":")
                            .Replace("\"sValor\":", "\"Value\":")
                            .Replace("\"bEncriptado\":", "\"Encrypt\":");
            return JsonFrom;
        }

        private string GetEnumValue(string s = null, int i = -1)
        {
            string value = "";

            if (s != string.Empty || s != null)
            {
                switch (s)
                {
                    case "AN":
                        value = "0";
                        break;
                    case "NE":
                        value = "1";
                        break;
                    case "NM":
                        value = "2";
                        break;
                    case "FD":
                        value = "3";
                        break;
                    case "HR":
                        value = "4";
                        break;
                    case "ND":
                        value = "5";
                        break;
                }

            }

            if (i != -1)
            {
                switch (i)
                {
                    case 0:
                        value = "AN";
                        break;
                    case 1:
                        value = "NE";
                        break;
                    case 2:
                        value = "NM";
                        break;
                    case 3:
                        value = "FD";
                        break;
                    case 4:
                        value = "HR";
                        break;
                    case 5:
                        value = "ND";
                        break;
                }
            }

            return value;
        }

        public static string RemoveAllNamespaces(string xmlDocument)
        {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

            return xmlDocumentWithoutNs.ToString();
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        private XmlDocument CreateSoapEnvelope(string service, string sXML)
        {
            string xml =
                 @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">" +
                 "<soap:Body>" +
                 "<" + service + @" xmlns=""http://www.pagoexpress.com.mx/pxUniversal"">"
                       + sXML +
                 "</" + service + ">" +
                 "</soap:Body>" +
                 "</soap:Envelope>";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(xml);
            return soapEnvelopeDocument;
        }

        public static string Encrypt(string sInput, string sKey)
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

        public static string Reverse(string v)
        {
            char[] chars = v.ToArray();
            string result = "";

            for (int i = 0, j = v.Length - 1; i < v.Length; i++, j--)
            {
                result += chars[j];
            }

            return result;
        }

        public static string Decrypt(string sInput, string sKey)
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