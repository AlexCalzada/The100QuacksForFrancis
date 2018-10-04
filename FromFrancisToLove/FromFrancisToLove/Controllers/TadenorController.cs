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

namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/TN")]
    public class TadenorController : Controller 
    {
        ClassTN TNClass = new ClassTN();
       // MyRelReq xmlReloadResponse = new MyRelReq();
        public readonly HouseOfCards_Context _context;

        public IActionResult GetDefault()
        {
            return Content("");
        }
        [HttpGet("Skus_TN")]
        public IActionResult Get()
        {
            //Regresa los SKUS
            return Ok(_context.catalogos_Productos);
        }

        public TadenorController(HouseOfCards_Context context)
        {
            _context = context;
        }

        [HttpPost("Recarga_TN")] 
        public IActionResult TN_Service(MyRelReq xmlData)
        {
            var service = "getReloadClass";
            var response = "ReloadResponse";
            //Determinamos el tipo de servicio Saldo o Datos
            //catalogos_Producto
            string[] prefixSku = xmlData.SKU.Split("-");
        
            var producto = _context.catalogos_Productos.First(a => a.SKU == xmlData.SKU);
            if (producto.IDProduct != "")
            {
              service = "getReloadData";
              response = "DataResponse";
              xmlData.ID_Product = producto.IDProduct;
            }
            xmlData.SKU = prefixSku[1];
            //  ResponseXml = TNClass.GetRespuesta(soapResult, service + "Result", response);

            var task = Task.Run(() =>{return GetResponse(service, xmlData);});

            MyRelReq ResponseXml = new MyRelReq();
            try
            {
                var success = task.Wait(50000);
                if (!success)
                {
                    //EDITAR
                    string codeResponse =  CR_TN_SERV(xmlData).ToString();
                    if (codeResponse.ToString() != "")
                    {
                        return Content("Lo sentimos El servicio tardo mas de los esperado :( "+codeResponse+":"+ EnumPrueba.TN_Cod(codeResponse));
                    }
                }
                else
                {
                    ResponseXml = TNClass.GetRespuesta(task.Result, service + "Result", response);
                }
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }

            // 71 o 6 
            if (ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71")
            {
                //EDITAR
                var codeResponse =  CR_TN_SERV(xmlData).ToString();
                if (EnumPrueba.TN_Cod(codeResponse)!= "")
                {
                    return Content(codeResponse);
                }
            }
            
            if (ResponseXml.ResponseCode != "0")
            {
                return Content(ResponseXml.DescripcionCode);
            }
           
         
            string Ticket =

                "NO. TRANSACCIÓN:  " + ResponseXml.TransNumber + Environment.NewLine + "NO. AUTORIZACIÓN: " + ResponseXml.AutoNo + Environment.NewLine +
                "MONTO:            " + producto.Monto + Environment.NewLine + "TELEFONO:         " + ResponseXml.PhoneNumber + Environment.NewLine +
                "FECHA Y HORA DE LA TRANSACCIÓN: " + ResponseXml.Datetime + Environment.NewLine +
                 ResponseXml.ResponseCode + ":" + ResponseXml.DescripcionCode;

            return Content(Ticket);
        }


        [HttpPost("Consultar_TN")]
        public IActionResult CR_TN_SERV(MyRelReq xmlData)
        {
            MyRelReq xmlRequest = new MyRelReq();
            var servicio = "getQueryClass";
            var response = "QueryResponse";
            //Si existe producto desde la BD lo agrega
            if (xmlData.ID_Product != null)
            {
                servicio = "getQueryDatClass";
                response = "DataQueryResponse";
                xmlRequest.ID_Product = xmlData.ID_Product;
                
            }
            xmlRequest.PhoneNumber = xmlData.ID_Product;
            xmlRequest.PhoneNumber = xmlData.PhoneNumber;
            xmlRequest.SKU = xmlData.SKU;
            xmlRequest.TC = xmlData.TC;
            xmlRequest.TransNumber = xmlData.TransNumber;

            MyRelReq ResponseXml = new MyRelReq();
            string xml = GetResponse(servicio, xmlRequest);
            ResponseXml = TNClass.GetRespuesta(xml, servicio + "Result", response);
            string CodeResponse =ResponseXml.ResponseCode;
            return Content(CodeResponse);
        }

        //Aqui sucede la magia :v
        public string GetResponse(string servicio,MyRelReq xmlQueryRequest)
        {
            var sXML = TNClass.GetXMLs(xmlQueryRequest, servicio);
            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = TNClass.CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/" + servicio, item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = TNClass.CreateSoapEnvelope(servicio, sXML);
            TNClass.InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            string soapResult="";
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

        protected void InsertSuccessfulTransaction(int Proveedor, string SKU, string Reference, decimal Amount, decimal Comision=0, long NTransaction=0, long NAutoritation=0)
        {
            var Transaction = _context.Set<Transaccion>();
            Transaction.Add(
                              new Transaccion
                              {

                                  FechaTx = DateTime.Now,
                                  Sku = SKU,
                                  NAutorizacion = NAutoritation,
                                  Referencia = Reference,
                                  Monto = Amount,
                                  Comision = Comision,
                                  ConfigID = Proveedor,
                                  TiendaID = 1,
                                  CajaID = 1,
                                  NoTransaccion = NTransaction
                              }
                           );
            _context.SaveChanges();
        }

    }
}