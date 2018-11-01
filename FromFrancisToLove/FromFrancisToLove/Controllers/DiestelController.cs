using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FromFrancisToLove.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Diestel;
using FromFrancisToLove.Requests.ModuleDiestel;
using FromFrancisToLove.ExtensionMethods;
using System.Reflection;
using FromFrancisToLove.Models;
using FromFrancisToLove.Diestel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Collections;
using FromFrancisToLove.Diestel.Clases;
using System.IO;
using static FromFrancisToLove.Payments.Backend.Services.Finfit;
using FromFrancisToLove.Payments.Backend.Services;

namespace FromFrancisToLove.Controllers
{
    //[Produces("application/json")]
    [Route("api/Diestel")]
    public class DiestelController : Controller
    {
        protected readonly HouseOfCards_Context _context;

        public DiestelController(HouseOfCards_Context context)
        {
            _context = context;
        }

        [HttpGet("RequestServiceDT")]
        public IActionResult RequestServiceDT([FromQuery]int User, [FromQuery]string SKU, [FromQuery]string Reference)
        {
            //ID del servicio
            int service = 0;

            //Separamos el prefijo y el sku
            string[] values = ExtensionMethods.ExtSKU.SeparateSku(SKU);

            string sPrefix = values[0];

            string sSKU = values[1];

            //Indicamos que servicio utilizar
            switch (sPrefix)
            {
                case "DT":
                    service = 1;
                    break;
                case "TN":
                    service = 2;
                    break;
            }

            //Obtenemos algun identificador del usuario que este solicitando el servicio
            string UserId = User.ToString();

            // Clase en donde se realizaran las operaciones en la Base de datos
            var crud = new DbCrud(_context);

            //La transaccion actual que viajara a travez de todos los metodos del WS
            long currentTransaction = 0;

            var isTransactionActive = crud.CheckTransaction(User);

            if (isTransactionActive.estatus == true && isTransactionActive.transactionStatus == 0)
            {
                //Obtenemos la transaccion "vacia" del usuario
                currentTransaction = isTransactionActive.currentTransaction;
            }
            else if(isTransactionActive.estatus == false || isTransactionActive.transactionStatus != 0)
            {
                //Se crea el registro de la transaccion del usuario por defecto vacia
                //Esto para obtener el numero de transaccion actual de la solicitud 
                //que se esta por procesar.
                var reg = crud.InsertInitialTransaction(User, SKU, Reference);

                //Validamos que se haya insertado el registro "vacio"
                if (!reg)
                {
                    return Content("La solicitud no pudo ser procesada");
                }
            }
                        
            //Aqui es Donde guardaremos el resultado final de este Action Method
            string result = "";
            
            //Conseguimos las credenciales
            var cnx = _context.conexion_Configs.Find(service);
            

            //validamos que servicio va hacer solicitado
            if (sPrefix == "DT")
            {
                //Almacenamos dentro de un arreglo los datos importantes para ejecutar el WS
                string[] data =
                {
                    sSKU,
                    Reference,
                    currentTransaction.ToString(),
                    cnx.Usr,
                    cnx.Pwd,
                    cnx.CrypKey
                };

                //Mandamos los datos por el constructor
                Diestel.Clases.RequestActiveService request = new Diestel.Clases.RequestActiveService(data);

                //Guardamos el resultado del WS
                var x = request.RequestService();

                var isUpdateSucessful = crud.UpdateUserTransaction(currentTransaction);

                if (isUpdateSucessful)
                {
                    result = x.response;
                    var success = crud.UpdateTxTest(result, currentTransaction);
                    if (success)
                    {
                        return Content(result);
                    }
                }
            }

            return Content(result);
        }
        
        [HttpPost("PayServiceDT")]
        public IActionResult PayServiceDT()
        {
            try
            {
                var reader = new StreamReader(Request.Body);

                var body = reader.ReadToEnd();

                string jsonContent = body;

                var root = JArray.Parse(jsonContent);
                var firstChild = JArray.Parse(root[0].ToString());
                dynamic fData = JObject.Parse(firstChild[0].ToString());

                int UserId = fData.Usuario;
                string wSKU = fData.SKU;
                string JReference = fData.Referencia;

                string[] values = ExtensionMethods.ExtSKU.SeparateSku(wSKU);
                string JPrefix = values[0];
                string JSku = values[1];

                
                int index = 0;
                
                switch (JPrefix)
                {
                    case "DT":
                        index = 1;
                        break;
                    case "TN":
                        index = 2;
                        break;
                }

                //La transaccion actual que viajara a travez de todos los metodos del WS
                long currentTransaction = 0;

                var crud = new DbCrud(_context);

                var isTransactionActive = crud.CheckTransaction(UserId);

                if (isTransactionActive.estatus == true && isTransactionActive.transactionStatus == 0)
                {
                    //Obtenemos la transaccion con "estatus 0" del usuario
                    currentTransaction = isTransactionActive.currentTransaction;
                }

                var dbconexion = _context.conexion_Configs.Find(index);

                if (JPrefix == "DT")
                {
                    List<cCampo> campos = null;

                    campos = JsonConvert.DeserializeObject<List<cCampo>>(root[1].ToString());


                    string[] data = new string[] 
                    {
                        dbconexion.Usr,
                        dbconexion.Pwd,
                        dbconexion.CrypKey,
                        currentTransaction.ToString()
                    };


                    Diestel.Clases.PayServiceDiestel psd = new Diestel.Clases.PayServiceDiestel(data);

                    var pay = psd.PayService(campos);

                    if (pay.response != string.Empty && pay.status == 1)
                    {
                        return Ok(pay.response);
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpGet("TestService")]
        public IActionResult TestService()
        {
            var dt = new Class_DT();

            var cnx = _context.conexion_Configs.Find(1);

            string[] credentials = { cnx.Url, "Info", cnx.Usr, cnx.Pwd, cnx.CrypKey };

            Field[] fields = new Field[3];
            fields[0] = new Field();
            fields[0].Name = "SKU";
            fields[0].Value = "8469760000019";
            fields[1] = new Field();
            fields[1].Name = "REFERENCIA";
            fields[1].Value = "8666346991";
            fields[2] = new Field();
            fields[2].Name = "TRANSACCION";
            fields[2].Value = 1;

            var listF = new List<Field>();
            foreach (var item in fields)
            {
                listF.Add(item);
            }


            var xml = dt.Send_Request("Info", credentials, listF);



            return Ok(xml);

        }        

        [HttpPost("Info")]
        public IActionResult Info(string SKU, string Reference)
        {
        //    int service = 0;

        //    switch (SKU.Split('-')[0].ToString())
        //    {
        //        case "DT":
        //            service = 1;
        //            break;
        //        case "TN":
        //            service = 2;
        //            break;
        //    }

        //    if (service == 0)
        //    {
        //        return NotFound();
        //    }

            var cnx = _context.conexion_Configs.Find(1);

            var Payments = new PaymentsService(cnx.Url, cnx.Usr, cnx.Pwd, cnx.CrypKey);
            Payments.Config(7, 1, 1, 1);

            var fields = Payments.PaymentInfo(SKU.Split('-')[1].ToString(), Reference);


            return Ok(fields);
        }

        [HttpPost("Ejecuta")]
        public IActionResult Ejecuta()
        {
            var dt = new Class_DT();
            var reader = new StreamReader(Request.Body);
            var body = reader.ReadToEnd();
            body = dt.ReplaceFrom(body);
            var root = JArray.Parse(body);
            
            var cnx = _context.conexion_Configs.Find(1);
            List<Field> fields = null;
            fields = JsonConvert.DeserializeObject<List<Field>>(root[0].ToString());

            var payments = new PaymentsService(cnx.Url, cnx.Usr, cnx.Pwd, cnx.CrypKey);
            payments.Config(7, 1, 1, 1, 1);

            var response = payments.Request(fields);
            return Ok(response);
        }
    }
}