﻿using System;
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


namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/TN")]
    public class TadenorController : Controller 
    {
        ClassTN TNClass = new ClassTN();
        MyRelReq xmlReloadResponse = new MyRelReq();
        public readonly HouseOfCards_Context _context;

        public IActionResult GetDefault()
        {
            return Ok("");
        }
        [HttpGet("Obtener_Skus")]
        public IActionResult Get()
        {
            //Regresa los SKUS
            return Ok(_context.catalogos_Productos);
        }

        public TadenorController(HouseOfCards_Context context)
        {
            _context = context;
        }

        [HttpPost("Recarga_Servicio")] 
        public IActionResult TN_Service(MyRelReq xmlData)
        {           
            var service = "getReloadClass";
            var response = "ReloadResponse";
            //Determinamos el tipo de servicio Saldo o Datos
            //catalogos_Producto
            var producto = _context.catalogos_Productos.First(a => a.SKU == xmlData.SKU && a.CONFIGID == 2);
            if (producto.IDProduct != "")
            {
              service = "getReloadData";
              response = "DataResponse";
            }
            MyRelReq ResponseXml = new MyRelReq();
            //  ResponseXml = TNClass.GetRespuesta(soapResult, service + "Result", response);
            ResponseXml = TNClass.GetRespuesta(GetResponse(service,xmlData), service + "Result", response);
            //Null 0 o 16 
            if (ResponseXml.ResponseCode == null || ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71")
            {
                ResponseXml.TC = xmlData.TC;
                ResponseXml.SKU = xmlData.SKU;
                ResponseXml.ID_Product = xmlData.ID_Product;
                xmlReloadResponse = ResponseXml;
                string x = EnumPrueba.TN_Cod(CR_TN_SERV(/*ResponseXml*/));
                if (x != "")
                {
                    return Content(x);
                }
            }
            
            if (ResponseXml.ResponseCode != "0")
            {
                return Content(ResponseXml.DescripcionCode);
            }
            //Tiket
            string Ticket =
"///////////////////////////////////////////////////////////////////////////////////////////////////////////" + Environment.NewLine +
"NO. TRANSACCIÓN:  " + ResponseXml.TransNumber + Environment.NewLine +
"NO. AUTORIZACIÓN: " + ResponseXml.AutoNo + Environment.NewLine +
"MONTO:            " + producto.Monto + Environment.NewLine +//Se consulta el SKU para traer el monto!!!!!!!!!!!!!
"TELEFONO:         " + ResponseXml.PhoneNumber + Environment.NewLine +
"///////////////////////////////////////////////////////////////////////////////////////////////////////////" + Environment.NewLine +
"TELCEL" + Environment.NewLine +
"ESTIMADO CLIENTE EN CASO DE PRESENTARSE ALGUN PROBLEMA CON SU TIEMPO AIRE FAVOR DE" + Environment.NewLine +
"COMUNICARSE A ATENCION A CLIENTES TELCEL *264 DESDE SU TELCEL O DESDE EL INTERIOR DE LA REPUBLICA" + Environment.NewLine +
"AL 01800-710-5687" + Environment.NewLine +
"///////////////////////////////////////////////////////////////////////////////////////////////////////////" + Environment.NewLine +
"FECHA Y HORA DE LA TRANSACCIÓN: " + ResponseXml.Datetime + Environment.NewLine +
"TIENDA:" + Environment.NewLine +
ResponseXml.ResponseCode + ":" +
ResponseXml.DescripcionCode + Environment.NewLine +
"///////////////////////////////////////////////////////////////////////////////////////////////////////////";
            return Content(Ticket);
        }

        [HttpPost("Consulta_Servicio")]
        public IActionResult TN_Consulta()
        {
            return Content("");
        }

        public string CR_TN_SERV(/*MyRelReq xmlReloadResponse*/)
        {
            MyRelReq xmlQueryRequest = new MyRelReq();
            
            xmlQueryRequest.PhoneNumber = xmlReloadResponse.PhoneNumber;
            xmlQueryRequest.SKU = xmlReloadResponse.SKU;
            xmlQueryRequest.TC = xmlReloadResponse.TC;
            xmlQueryRequest.TransNumber = xmlReloadResponse.TransNumber;

            var servicio = "getQueryClass";
           
            if (xmlReloadResponse.ID_Product!="")
            {
                xmlQueryRequest.ID_Product = xmlReloadResponse.ID_Product;
                servicio = "getQueryDatClass";
            }
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(GetResponse(servicio,xmlQueryRequest));
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(servicio + "Result");

            string xml = "";
            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xmldoc.LoadXml(TNClass.Des_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            string x="";
            foreach (XmlNode node in nodeList)
            {
                x = node.InnerText;
            }
            return x;
        }

        public string GetResponse(string servicio,MyRelReq xmlQueryRequest)
        {
            var sXML = TNClass.GetXMLs(xmlQueryRequest, servicio);
            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = TNClass.CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/" + servicio, item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = TNClass.CreateSoapEnvelope(servicio, sXML);
            TNClass.InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            WebResponse webResponse = webRequest.EndGetResponse(asyncResult);
            StreamReader rd = new StreamReader(webResponse.GetResponseStream());
             return rd.ReadToEnd();
        }
    }
}