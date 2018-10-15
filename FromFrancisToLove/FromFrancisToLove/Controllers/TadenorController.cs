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
namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/TN")]
    public class TadenorController : Controller
    {
        public TadenorController(HouseOfCards_Context context)
        {
            _context = context;
        }



        ClassTN TNClass = new ClassTN();
        // MyRelReq xmlReloadResponse = new MyRelReq();
        public readonly HouseOfCards_Context _context;

        

        public IActionResult GetDefault()
        {
            return Content("");
        }

        [HttpPost("Skus_TN")]
        public IActionResult Get_Skus()
        {

            var reader = new StreamReader(Request.Body);
            var Body = reader.ReadToEnd();
            string JsonContent = Body;
            dynamic data = JObject.Parse(JsonContent);
            string Sku = data.SKU;
            var item = _context.catalogos_Productos.First(b => b.SKU == Sku);
            string respuesta =
                "[" +
                "{\"sCampo\":\"SKU\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\""+item.SKU+"\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"IdProduct\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\""+item.IDProduct + "\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"Monto\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.Monto + "\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"Marca\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.Marca + "\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"CONFIGID\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.CONFIGID + "\", \"bEncriptado\":true}" +
                "]";
            return Content(respuesta);

           

/*
            var reader = new StreamReader(Request.Body);
            var Body = reader.ReadToEnd();
            string JsonContent = Body;
            var ArrP = JArray.Parse(JsonContent);
            var ArrH = JArray.Parse(ArrP[0].ToString());
            foreach (var sJson in ArrH)
            {
                dynamic data = JObject.Parse(sJson.ToString());

                string Sku = data.SKU;
               
                    var item = _context.catalogos_Productos.First(b => b.SKU == Sku);
                    return Content("{\"" + nameof(item.SKU) + "\":\"" + item.SKU + "\", \"IdProduct\":\"" + item.IDProduct + "\", \"Monto\":\"" + item.Monto + "\", \"Marca\":\"" + item.Marca + "\", \"CONFIGID\":\"" + item.CONFIGID + "\"}");
                
            }
            return Content("");
          */

            
        
         


        }

        [HttpPost("{id}/{sku}/{reference}")]
        public IActionResult Get_Sku(string sku,string reference="")
        {
            
            try
            {
                var item = _context.catalogos_Productos.First(a => a.SKU == sku);
                return Content("{\""+nameof(item.SKU)+"\":\"" + item.SKU + "\", \"IdProduct\":\"" + item.IDProduct + "\", \"Monto\":\"" + item.Monto + "\", \"Marca\":\"" + item.Marca + "\", \"CONFIGID\":\"" + item.CONFIGID + "\"}");
            }
            catch (Exception)
            {

                throw;
            }
             
        
         
        }

        [HttpPost("ALL_Skus")]
        public IActionResult Get(string id)
        {
            string Skus = "[";
            foreach (var item in _context.catalogos_Productos)
            {
            Skus +=
            "[" +
            "{\"sCampo\":\"SKU\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.SKU + "\", \"bEncriptado\":true}," +
            "{\"sCampo\":\"IdProduct\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.IDProduct + "\", \"bEncriptado\":true}," +
            "{\"sCampo\":\"Monto\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.Monto + "\", \"bEncriptado\":true}," +
            "{\"sCampo\":\"Marca\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.Marca + "\", \"bEncriptado\":true}," +
            "{\"sCampo\":\"CONFIGID\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.CONFIGID + "\", \"bEncriptado\":true}" +
            "],";
            }
           Skus= Skus.Remove(Skus.Length - 1);
            Skus += "]";
            //Regresa los SKUS
            return Content(Skus);

        }


        MyRelReq xmlData = new MyRelReq();
        [HttpPost("Recarga_TN")]
        public IActionResult TN_Service()
        {
          
            var reader = new StreamReader(Request.Body);
            var Body = reader.ReadToEnd();
            string JsonContent = Body;
            var ArrP = JArray.Parse(JsonContent);
            var ArrH = JArray.Parse(ArrP[0].ToString());
            dynamic data = JObject.Parse(ArrH[0].ToString());
            xmlData.SKU = data.SKU;
            xmlData.PhoneNumber = data.Referencia;

            string service = "getReloadClass";
            string response = "ReloadResponse";
            //Determinamos el tipo de servicio Saldo o Datos
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

            string[] credentials = new string[] {item.Url, item.Usr, item.Pwd};

            var task = Task.Run(() =>{return TNClass.Send_Request(service, xmlData, credentials);});
            MyRelReq ResponseXml = new MyRelReq();
            try
            {
                var success = task.Wait(50000);
                if (!success)
                {
                return CR_TN_SERV(ResponseXml);
                }
                else
                {
                ResponseXml = TNClass.Deserializer_Response(task.Result, service + "Result", response);
                }
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            // 71 o 6 
            if (ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71")
            {
                return CR_TN_SERV(ResponseXml);
            }
           
            
            return Content(TNClass.jString(ResponseXml));
        } 



        [HttpPost("Consultar_TN")]
        public IActionResult CR_TN_SERV(MyRelReq ResponseXml)
        {         
            var reader = new StreamReader(Request.Body);
            var Body = reader.ReadToEnd();
            string JsonContent = Body;
            var ArrP = JArray.Parse(JsonContent);
            var ArrH = JArray.Parse(ArrP[0].ToString());
            dynamic data = JObject.Parse(ArrH[0].ToString());
            xmlData.SKU = data.SKU;
            xmlData.PhoneNumber = data.Referencia;
           
          
      
            // string a = Metodo(xmlData);
            string service = "getQueryClass";
            string response = "QueryResponse";
            //Si existe producto desde la BD lo agrega
            if (xmlData.ID_Product != null)
            {
                service = "getQueryDatClass";
                response = "DataQueryResponse";
                
            }
         //   MyRelReq ResponseXml = new MyRelReq();
            
            xmlData.Datetime= DateTime.Now;
            var item = _context.conexion_Configs.Find(2);
            //string xml = GetResponse(service, xmlData);

            string[] credentials = new string[] { item.Url, item.Usr, item.Pwd };
            var task = Task.Run(() => { return TNClass.Send_Request(service, xmlData,credentials);});
            try
            {
                var success = task.Wait(50000);
                if (!success)
                {
                  return Content("Lo sentimos el servicio tardo mas de los esperado :( ");   
                }
                else
                {
                    ResponseXml = TNClass.Deserializer_Response(task.Result, service + "Result", response);
                }
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }     
            return Content(TNClass.jString(ResponseXml));
        }

        protected void SaveXML(MyRelReq xmlDatos)
        {
            var Transaction = _context.Set<Transaccion2>();
            Transaction.Add(
                              new Transaccion2
                              {
                                  SKU = xmlDatos.SKU,
                                  ID_GRP = xmlDatos.ID_GRP,
                                  ID_CHAIN = xmlDatos.ID_CHAIN,
                                  ID_MERCHANT = xmlDatos.ID_MERCHANT,
                                  ID_COUNTRY = xmlDatos.ID_COUNTRY,
                                  ID_POS = xmlDatos.ID_POS,
                                  ID_Product = xmlDatos.ID_Product,
                                  Referencia= xmlDatos.PhoneNumber,
                                  ID_CONFIG= 2,
                                  Fecha=xmlDatos.Datetime,
                                  TransNumber=xmlDatos.TransNumber,
                                  TC=xmlDatos.TC,
                                  Brand=xmlDatos.Brand,
                                  Instr1=xmlDatos.Instr1,
                                  Instr2=xmlDatos.Instr2,
                                  ResponseCode=xmlDatos.ResponseCode,
                                  DescripcionCode=xmlDatos.DescripcionCode,
                                  AutoNo=xmlDatos.AutoNo,
                                  Monto=Decimal.Parse(xmlDatos.Monto)
                              }
                             
                            
                           );
            _context.SaveChanges();
        }
    }


}
