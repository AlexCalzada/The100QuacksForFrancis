using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Diestel;
using FromFrancisToLove.Data;
using FromFrancisToLove.Models;
using FromFrancisToLove.Requests.ModuleDiestel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static FromFrancisToLove.Payments.Backend.Services.Finfit;

namespace FromFrancisToLove.Controllers
{
   // [Produces("application/json")]
    [Route("api/Services")]
    public class ServicesController : Controller
    {
        private readonly HouseOfCards_Context _context;
        public ServicesController(HouseOfCards_Context context)
        {
            _context = context;
        }

        // GET: api/Services
        [HttpGet("GetSKUs")]
        public IActionResult Get()
        {
            return Ok(_context.catalogos_Productos);
        }

        [HttpPost("RequestService")]
        public IActionResult Put(string SKU,string Reference)
        {
            int id_credentials=0;
            switch (SKU.Split("-")[0].ToString())
            {
                case "DT":
                    id_credentials = 1;
                    break;
                case "TN":
                    id_credentials = 2;
                    break;
                
            }

            if (id_credentials==0)
            {
                return Ok("");
            }          
                var cnx = _context.conexion_Configs.Find(id_credentials);
                PaymentsService Payment = new PaymentsService(cnx.Url, cnx.Usr, cnx.Pwd,cnx.CrypKey);
                Payment.Config(7, 1, 1, 1);

                var fields = Payment.PaymentInfo(SKU, Reference);
                return Ok(Payment.PaymentInfo(SKU, Reference));

        }
        [HttpPost("PayService")]
        public IActionResult PayService()
        {
           

            var reader = new StreamReader(Request.Body);
            var body = reader.ReadToEnd();
            string jsonContent = body;

            var root = JArray.Parse(jsonContent);
            var firstChild = JArray.Parse(root[0].ToString());
            var fields = root[1].ToString();

            dynamic jdata = JObject.Parse(firstChild[0].ToString());

            string jSKU = jdata.SKU;
            string[] SKU = ExtensionMethods.ExtSKU.SeparateSku(jSKU);

            Conexion_Config cnx = null;
            ResponseService response = null;

            var lsFields = JsonConvert.DeserializeObject<List<Field>>(fields);


            int id_credentials = 0;

            switch (SKU[0])
            {
                case "DT":
                    id_credentials = 1;
                    break;
                case "TN":
                    id_credentials = 2;
                    break;
            }
            cnx = _context.conexion_Configs.Find(id_credentials);
            var credentials = new PaymentsService(cnx.Url, cnx.Usr, cnx.Pwd, cnx.CrypKey);
            credentials.Config(7, 1, 1, 1);
            response = credentials.Request(lsFields);
            return Ok(response);
        }


        [HttpPost("CheckService")]
        public IActionResult CheckService()
        {
            var reader = new StreamReader(Request.Body);
            var body = reader.ReadToEnd();
            string jsonContent = body;

            var root = JArray.Parse(jsonContent);
            var firstChild = JArray.Parse(root[0].ToString());
            var fields = root[1].ToString();

            dynamic jdata = JObject.Parse(firstChild[0].ToString());

            string jSKU = jdata.SKU;
            string[] SKU = ExtensionMethods.ExtSKU.SeparateSku(jSKU);

            Conexion_Config cnx = null;
            ResponseService response = null;

            var lsFields = JsonConvert.DeserializeObject<List<Field>>(fields);


            int id_credentials = 0;

            switch (SKU[0])
            {
                case "DT":
                    id_credentials = 1;
                    break;
                case "TN":
                    id_credentials = 2;
                    break;
            }
            cnx = _context.conexion_Configs.Find(id_credentials);
            var credentials = new PaymentsService(cnx.Url, cnx.Usr, cnx.Pwd, cnx.CrypKey);
            credentials.Config(7, 1, 1, 1);
            response = credentials.Check(lsFields);
            return Ok(response);
        }


 


    }
}
