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
            string[] values = ExtSKU.SeparateSku(SKU);

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
                RequestActiveService request = new RequestActiveService(data);

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

                string[] values = ExtSKU.SeparateSku(wSKU);
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

                    
                    PayServiceDiestel psd = new PayServiceDiestel(data);

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
            try
            {
                DbCrud crud = new DbCrud(_context);
                var test = crud.CheckTransaction(1);
                return Ok($"Transaccion actual: {test.currentTransaction}| Estado: {test.estatus}");
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }        
    }
}