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
using FromFrancisToLove.ServiceInfo;
using FromFrancisToLove.Diestel;
using FromFrancisToLove.Diestel.Clases;

namespace FromFrancisToLove.Controllers
{
    //[Produces("application/json")]
    [Route("api/Diestel")]
    public class DiestelController : Controller
    {
        protected readonly HouseOfCards_Context _context;
        protected readonly PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);
        protected ServiceInformation dataInfo = new ServiceInformation();

        public DiestelController(HouseOfCards_Context context)
        {
            _context = context;
        }

        public void GetServiceData(string _SKU, string Reference)
        {
            try
            {
                //Se obtiene el proveedor del servicio
                dataInfo.ProviderId = _context.catalogos_Productos
                                      .Where(x => x.SKU == _SKU)
                                      .SingleOrDefault().CONFIGID;

                //Se establece el SKU y Referencia recibidos
                dataInfo.SKU = _SKU;
                dataInfo.Reference = Reference;

                if (dataInfo.ProviderId > 0)
                {
                    //Credenciales
                    dataInfo.User = _context.conexion_Configs
                                    .Where(x => x.ConfigID == dataInfo.ProviderId)
                                    .SingleOrDefault().Usr;
                    dataInfo.Password = _context.conexion_Configs
                                        .Where(x => x.ConfigID == dataInfo.ProviderId)
                                        .SingleOrDefault().Pwd;
                    //Clave de Encriptacion
                    dataInfo.EncryptionKey = _context.conexion_Configs
                                             .Where(x => x.ConfigID == dataInfo.ProviderId)
                                             .SingleOrDefault().CrypKey;
                    //Url del servicio
                    dataInfo.URL = _context.conexion_Configs
                                   .Where(x => x.ConfigID == dataInfo.ProviderId)
                                   .SingleOrDefault().Url;
                    //Ultima transaccion realizada
                    long lastTransaction;
                    try
                    {
                        lastTransaction = _context.transaccions
                                           .OrderByDescending(x => x.NoTransaccion)
                                           .FirstOrDefault().NoTransaccion;
                    }
                    catch (Exception)
                    {
                        lastTransaction = 1;
                    }
                    if (lastTransaction == null || lastTransaction == 0)
                    {
                        dataInfo.Transaccion = 1;
                    }
                    else
                    {
                        dataInfo.Transaccion = lastTransaction + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                dataInfo.ProviderId = -1;
            }
        }

        private void GetValuesFromJson(string JsonString)
        {
            var jarr = JArray.Parse(JsonString);
            foreach (var jarrItem in jarr.Children<JObject>())
            {
                foreach (var propertyItem in jarrItem.Properties())
                {
                    string propertyName = propertyItem.Name;
                    if (propertyName == "SKU")
                    {
                        string propertyValue = (string)propertyItem.Value;
                        Content($"Nombre: {propertyName}, Valor: {propertyValue} </br>");
                    }
                }
            }
        }

        // GET: api/Diestel
        //[HttpGet]
        //public IActionResult Get()
        //{
        //    var response = _context.catalogos_Productos.ToList();
        //    return Ok(response);
        //}

        // GET: api/Diestel/5
        [HttpGet("RequestService")]
        public IActionResult RequestService([FromQuery]string SKU, [FromQuery]string Reference)
        {
            /* 
               Se obtienen los datos del servicio a utilizar
               de acuerdo al SKU y Referencia recibidos
            */
            GetServiceData(SKU, Reference);

            string jsonResult = "Default Message";

            if (dataInfo.ProviderId == (int)Provider.Diestel)
            {
                cCampo[] requestInfo = new cCampo[10];

                //8469760000187
                //8469761001749

                //modulo.TokenValor = "1020304050";

                if (dataInfo.SKU == string.Empty)
                {
                    return BadRequest($"No se ha especificado el servicio a pagar");
                }

                try
                {
                    requestInfo[0] = new cCampo();
                    requestInfo[0].iTipo = eTipo.NE;
                    requestInfo[0].sCampo = "IDGRUPO";
                    requestInfo[0].sValor = dataInfo.Grupo;

                    requestInfo[1] = new cCampo();
                    requestInfo[1].iTipo = eTipo.NE;
                    requestInfo[1].sCampo = "IDCADENA";
                    requestInfo[1].sValor = dataInfo.Cadena;

                    requestInfo[2] = new cCampo();
                    requestInfo[2].iTipo = eTipo.NE;
                    requestInfo[2].sCampo = "IDTIENDA";
                    requestInfo[2].sValor = dataInfo.Tienda;

                    requestInfo[3] = new cCampo();
                    requestInfo[3].iTipo = eTipo.NE;
                    requestInfo[3].sCampo = "IDPOS";
                    requestInfo[3].sValor = dataInfo.POS;

                    requestInfo[4] = new cCampo();
                    requestInfo[4].iTipo = eTipo.NE;
                    requestInfo[4].sCampo = "IDCAJERO";
                    requestInfo[4].sValor = dataInfo.Cajero;

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
                    requestInfo[7].sValor = dataInfo.Transaccion;

                    requestInfo[8] = new cCampo();
                    requestInfo[8].iTipo = eTipo.AN;
                    requestInfo[8].sCampo = "SKU";
                    requestInfo[8].sValor = dataInfo.SKU;

                    if (dataInfo.Reference != string.Empty)
                    {
                        requestInfo[9] = new cCampo();
                        requestInfo[9].sCampo = "REFERENCIA";
                        requestInfo[9].sValor = Encriptacion.PXEncryptFX(dataInfo.Reference, dataInfo.EncryptionKey);
                        requestInfo[9].bEncriptado = true;
                        requestInfo[9].iTipo = eTipo.AN;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error dentro de la parametrización. {ex}");
                }

                try
                {
                    if (dataInfo.User == string.Empty || dataInfo.Password == string.Empty)
                    {
                        return BadRequest("Imposible conectar al WS porque no hay credenciales");
                    }
                    else
                    {
                        wservice.ClientCredentials.UserName.UserName = dataInfo.User;
                        wservice.ClientCredentials.UserName.Password = dataInfo.Password;
                    }

                    var response = wservice.InfoAsync(requestInfo).Result;

                    if (response.GetUpperBound(0) > 0)
                    {
                        // Validacion de respuesta del servicio
                        int codeResponse = 0;

                        if (response[0].sCampo == "CODIGORESPUESTA")
                        {
                            string codeDescription;
                            codeResponse = (int)response[0].sValor;

                            if (codeResponse == (int)response[0].sValor)
                            {
                                if (response[1].sCampo == "CODIGORESPUESTADESCR")
                                {
                                    codeDescription = response[1].sValor.ToString();
                                    return Ok("[{\"Error\":\"" + codeResponse + ":" + codeDescription + "\"}]");
                                }
                                return StatusCode(503);
                            }
                        }
                    }
                    else if (response.GetUpperBound(0) <= 0)
                    {
                        return StatusCode(204);
                    }

                    if (response.GetUpperBound(0) > 0)
                    {
                        int count = response.Length;

                        // Validacion de la referrencia, confirmacion
                        if (response[1].iLongitud.ToString() == "10")
                        {
                            //modulo.Confirmacion = true;
                            dataInfo.TelReference = response[1].sValor.ToString();

                            try
                            {
                                dataInfo.ReferenceConfirm = Encriptacion.PXDecryptFX(dataInfo.Reference, dataInfo.EncryptionKey);
                            }
                            catch (Exception)
                            {
                                dataInfo.ReferenceConfirm = string.Empty;
                                return NotFound(dataInfo.ReferenceConfirm.ToString());
                            }
                        }

                        var list = new List<object>();
                        foreach (cCampo wsCampo in response)
                        {
                            if (wsCampo.sCampo.ToString() == "COMISION")
                            {
                                dataInfo.Comision = (decimal)wsCampo.sValor;
                            }
                            if (wsCampo.sCampo.ToString() == "TOKEN")
                            {
                                dataInfo.Token = wsCampo.sValor.ToString();
                            }
                            if (wsCampo.bEncriptado == true)
                            {
                                wsCampo.sValor = Encriptacion.PXDecryptFX(wsCampo.sValor.ToString(), dataInfo.EncryptionKey);
                            }

                            list.Add(wsCampo);
                        }

                        //Se incrementa el numero de transacción

                        var jsonResponse = JsonConvert.SerializeObject(list);
                        jsonResult = jsonResponse;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            else if (dataInfo.ProviderId < 0)
            {
                jsonResult = "[{\"Error\":\"No se encontró el proveedor solicitado\"}]";
            }

            return Ok(jsonResult);
        }

        
        // POST: api/Diestel
        [HttpPost("PayService")]
        public IActionResult PayService(ServiceInformation data)
        {
            var jarr = JArray.Parse(data.JSON);

            string Prefix = "";
            string sku = "";
            string refe = "";

            foreach (var jarrItem in jarr.Children<JObject>())
            {
                foreach (var propertyItem in jarrItem.Properties())
                {
                    string propertyName = propertyItem.Name;
                    string propertyValue = (string)propertyItem.Value;

                    switch (propertyName)
                    {
                        case "SKU":
                            string[] s = propertyValue.Split('-');
                            for (int i = 0; i < s.Length - 1; i++)
                            {

                                if (s[0].Length == 2)
                                {
                                    Prefix = s[0];
                                    if (s[1].Length == 13)
                                    {
                                        sku = s[1];
                                    }
                                }
                            }
                            break;
                        case "Referencia":
                            refe = propertyValue;
                            break;
                    }

                }
            }

            var jj = JsonConvert.DeserializeObject<List<cCampo>>((string)jarr[1]);
            switch (Prefix)
            {
                case "DT":
                    jj = JsonConvert.DeserializeObject<List<cCampo>>((string)jarr[1]);
                    break;
            }

            //return Ok($"Prefijo: {Prefix} | SKU: {sku} | Referencia: {refe}");

            // Se almacenan los valores del JSON recibido
            var _SKU = sku;
            var _Ref = refe;
            var js = jj;

            //return Ok($"SKU: {_SKU} | Referencia: {_Ref} | Proveedor: {_Provider}");

            // Se deserealiza el JSON recibido
            //var js = JsonConvert.DeserializeObject<List<cCampo>>(data.JSON);

            //----------------------------------------------------------

            //Se declara una variable para almacenar la cantidad
            //de elementos dentro del json recibido
            int elementsAmount = js.Count();

            //----------------------------------------------------------

            //Se crea una copia de la cantidad de elementos recibidos
            int responseFields = elementsAmount;

            // Diez es la cantidad de elementos fijos (estaticos)
            // que siempre van a viajar en cada solicitud del WebService
            // a esa cantidad se le suman la cantidad de elementos recibidos
            // 
            int index = 10 + responseFields;

            //----------------------------s------------------------------
            var jsonResult = "Default Message";

            GetServiceData(_SKU, _Ref);

            if (dataInfo.SKU == null || dataInfo.SKU == "")
            {
                return BadRequest("No hay datos");
            }

            cCampo [] requestEjecuta = new cCampo[index];
            var Reversos = new CancelLog();

            //8469760000187
            //8469761001749

            try
            {
                requestEjecuta[0] = new cCampo();
                requestEjecuta[0].iTipo = eTipo.NE;
                requestEjecuta[0].sCampo = "IDGRUPO";
                requestEjecuta[0].sValor = dataInfo.Grupo;

                requestEjecuta[1] = new cCampo();
                requestEjecuta[1].iTipo = eTipo.NE;
                requestEjecuta[1].sCampo = "IDCADENA";
                requestEjecuta[1].sValor = dataInfo.Cadena;

                requestEjecuta[2] = new cCampo();
                requestEjecuta[2].iTipo = eTipo.NE;
                requestEjecuta[2].sCampo = "IDTIENDA";
                requestEjecuta[2].sValor = dataInfo.Tienda;

                requestEjecuta[3] = new cCampo();
                requestEjecuta[3].iTipo = eTipo.NE;
                requestEjecuta[3].sCampo = "IDPOS";
                requestEjecuta[3].sValor = dataInfo.POS;

                requestEjecuta[4] = new cCampo();
                requestEjecuta[4].iTipo = eTipo.NE;
                requestEjecuta[4].sCampo = "IDCAJERO";
                requestEjecuta[4].sValor = dataInfo.Cajero;

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
                requestEjecuta[8].sValor = dataInfo.Transaccion;

                /* Actualizar el ticket de la base de datos */
                //UpdateTx(1, (long)campo[8].sValor);

                requestEjecuta[9] = new cCampo();
                requestEjecuta[9].iTipo = eTipo.AN;
                requestEjecuta[9].sCampo = "SKU";
                requestEjecuta[9].sValor = dataInfo.SKU;

                //----------------------------------------------------------

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
                            if (cp <= responseFields)
                            {
                                //Agregamos los nuevos elementos del arreglo recibido al
                                //arreglo principal
                                requestEjecuta[j] = new cCampo();
                                requestEjecuta[j].sCampo = js[cp].sCampo;

                                //Se filtran los elemntos que deberan ir encriptados
                                if (js[cp].sCampo == "REFERENCIA")
                                {
                                    requestEjecuta[j].sValor = Encriptacion.PXEncryptFX(js[cp].sValor.ToString(), dataInfo.EncryptionKey);
                                    requestEjecuta[j].bEncriptado = true;
                                }
                                else
                                {
                                    requestEjecuta[j].sValor = js[cp].sValor.ToString();
                                    requestEjecuta[j].bEncriptado = false;
                                }

                                //Se incrementa la variable para pasar al siguiente elemento que se debera
                                // de añadir al arreglo
                                cp++;
                            }
                        }
                    }
                }

                //var MyTX = new TXDiestel();
                //MyTX.setSKU(dataInfo.SKU.Trim());
                //MyTX.AddToRequest(new DiestelField("NE", 4, "IDGRUPO"));
                //MyTX.AddToRequest(new DiestelField("NE", 5, "IDCADENA"));
                //MyTX.AddToRequest(new DiestelField("NE", 5, "IDTIENDA"));
                //MyTX.AddToRequest(new DiestelField("NE", 4, "IDPOS"));
                //MyTX.AddToRequest(new DiestelField("NE", 10, "IDCAJERO"));
                //MyTX.AddToRequest(new DiestelField("AN", 10, "FECHALOCAL"));
                //MyTX.AddToRequest(new DiestelField("AN", 10, "HORALOCAL"));
                //MyTX.AddToRequest(new DiestelField("NE", 12, "TRANSACCION"));
                //MyTX.AddToRequest(new DiestelField("AN", 13, "SKU"));
                //MyTX.AddToRequest(new DiestelField("AN", 10, "FECHACONTABLE"));

                /*-------------------------------------------------------------*/

                try
                {
                    if (dataInfo.User == string.Empty || dataInfo.Password == string.Empty || dataInfo.User == null || dataInfo.Password == null)
                    {
                        return BadRequest("Imposible conectar al WS porque no hay credenciales");
                    }
                    else
                    {
                        wservice.ClientCredentials.UserName.UserName = dataInfo.User;
                        wservice.ClientCredentials.UserName.Password = dataInfo.Password;
                    }

                    var response = wservice.EjecutaAsync(requestEjecuta).Result;
                    Reversos.WriteTXData("+", "Solicitud de Pago enviada", dataInfo.Transaccion.ToString());

                    if (response.GetUpperBound(0) > 0)
                    {
                        // Validacion de respuesta del servicio

                        if (response[0].sCampo == "CODIGORESPUESTA")
                        {
                            string codeDescription;
                            int codeResponse = (int)response[0].sValor;

                            if (codeResponse == (int)response[0].sValor)
                            {
                                if (response[1].sCampo == "CODIGORESPUESTADESCR")
                                {
                                    codeDescription = response[1].sValor.ToString();
                                    Reversos.WriteTXData(codeResponse.ToString(), codeDescription, "1");
                                    //return Ok($"Error {codeResponse}: {codeDescription}");
                                    return Ok("[{\"Error\":\"" + codeResponse +":"+ codeDescription + "\"}]");
                                }
                            }
                        }
                    }

                    if (response.GetUpperBound(0) > 0)
                    {
                        // Validacion de la referrencia, confirmacion
                        //if (response[1].iLongitud.ToString() == "10")
                        {

                            dataInfo.TelReference = response[1].sValor.ToString();
                            foreach (cCampo c in response)
                            {
                                if (c.sCampo == "AUTORIZACION")
                                {
                                    dataInfo.NumeroAutorizacion = long.Parse(c.sValor.ToString());
                                }
                                //SE RECOPILA LOS DATOS DE LA RESPUESTA
                                //MyTX.AddToResponse(new DiestelField(c.iTipo.ToString(), c.iLongitud, c.sCampo));
                            }

                            try
                            {
                                dataInfo.ReferenceConfirm = Encriptacion.PXDecryptFX(dataInfo.TelReference, dataInfo.EncryptionKey);
                            }
                            catch (Exception)
                            {
                                return NotFound(dataInfo.ReferenceConfirm.ToString());
                            }

                            var list = new List<object>();
                            foreach (cCampo wsCampo in response)
                            {
                                if (wsCampo.sCampo.ToString() == "COMISION")
                                {
                                    dataInfo.Comision = (decimal)wsCampo.sValor;
                                }
                                if (wsCampo.sCampo.ToString() == "TOKEN")
                                {
                                    dataInfo.Token = wsCampo.sValor.ToString();
                                }
                                if (wsCampo.sCampo == "REFERENCIA")
                                {
                                    wsCampo.sValor = Encriptacion.PXDecryptFX(wsCampo.sValor.ToString(), dataInfo.EncryptionKey);
                                }

                                list.Add(wsCampo);
                            }

                            try
                            {
                                //Se registra la transaccion
                                InsertSuccessfulTransaction(dataInfo.SKU, dataInfo.NumeroAutorizacion, dataInfo.Reference, dataInfo.Monto, dataInfo.Comision, dataInfo.Transaccion);
                            }
                            catch (Exception ex)
                            {
                                return BadRequest();
                            }

                            var jsonResponse = JsonConvert.SerializeObject(list);
                            jsonResult = jsonResponse;
                        }

                        return Ok(jsonResult);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("[{\"Error\":\"" + ex + "\"}]");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("[{\"Error\":\"" + ex + "\"}]");
            }

            return NotFound();
            
        }

        protected void InsertSuccessfulTransaction(string SKU, long NAutoritation, string Reference, decimal Amount, decimal Comision, long NTransaction)
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
                                  ConfigID = 1,
                                  TiendaID = dataInfo.Tienda,
                                  CajaID = dataInfo.Cajero,
                                  NoTransaccion = NTransaction
                              }
                           );
            _context.SaveChanges();
        }

        [HttpGet("CancelPayment")]
        public IActionResult CancelPayment([FromQuery]string SKU, [FromQuery]string Reference, [FromQuery] string Authorization)
        {
            try
            {
                GetServiceData(SKU, Reference);
                TXDiestel xDiestel = new TXDiestel(dataInfo.SKU, dataInfo.Reference, Authorization, dataInfo.User, dataInfo.Password, dataInfo.EncryptionKey);
                xDiestel.ExecuteReverseProcess("999", "Solicitud de cancelacion manual enviada.", "1");
                return Ok("Cancelación efectuada. Revise el archivo.");
            }
            catch (Exception)
            {
                return BadRequest("Errores en la petición.");
                throw;
            }
        }


        /*----------------------------------------------*/


        //private void UpdateTx(int Id, long NoTX)
        //{
        //    try
        //    {
        //        var tx = new ws_Diestel() { ws_id = Id, ws_no_tx = NoTX};

        //        _context.Attach(tx);
        //        _context.Entry(tx).Property(x => x.ws_no_tx).IsModified = true;
        //        _context.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}
        
        public bool InsertRequestFields(DiestelField diestelField)
        {
            try
            {

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool DeleteRequestFields()
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public bool DeleteResponseFields()
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public bool UpdateFields()
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        // PUT: api/Diestel/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
