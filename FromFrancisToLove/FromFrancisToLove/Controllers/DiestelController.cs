﻿using System;
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

namespace FromFrancisToLove.Controllers
{
    //[Produces("application/json")]
    [Route("api/Diestel")]
    public class DiestelController : Controller
    {
        private readonly HouseOfCards_Context _context;
        private readonly PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);

        /*Campos de texto (prueba)*/
        private List<object> objList = new List<object>();
        string txtNoTransaccion;
        string telReference;

        public DiestelController(HouseOfCards_Context context)
        {
            _context = context;
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
            var credentials = _context.conexion_Configs.Find(1);
            //var producto = _context.catalogos_Productos.Find(16);
            

            var modulo = new ModuleTDV();

            string formaPago = TipoPago.Efectivo.AsText();

            cCampo[] campo = new cCampo[10];

            modulo.Grupo = 7;
            modulo.Cadena = 1;
            modulo.Tienda = 1;
            modulo.POS = 1;
            modulo.Cajero = 1;

            modulo.Usuario = credentials.Usr;
            modulo.Password = credentials.Pwd;

            //8469760000187
            //8469761001749

            //modulo.SKU = "8469760000187";
            modulo.SKU = SKU;
            var referencia = Reference;

            modulo.EncriptionKey = credentials.CrypKey;

            modulo.TokenValor = "1020304050";

            if (modulo.SKU == string.Empty)
            {
                return BadRequest($"No se ha especificado el servicio a pagar");
            }

            try
            {
                campo[0] = new cCampo();
                campo[0].iTipo = eTipo.NE;
                campo[0].sCampo = "IDGRUPO";
                campo[0].sValor = modulo.Grupo;

                campo[1] = new cCampo();
                campo[1].iTipo = eTipo.NE;
                campo[1].sCampo = "IDCADENA";
                campo[1].sValor = modulo.Cadena;

                campo[2] = new cCampo();
                campo[2].iTipo = eTipo.NE;
                campo[2].sCampo = "IDTIENDA";
                campo[2].sValor = modulo.Tienda;

                campo[3] = new cCampo();
                campo[3].iTipo = eTipo.NE;
                campo[3].sCampo = "IDPOS";
                campo[3].sValor = modulo.POS;

                campo[4] = new cCampo();
                campo[4].iTipo = eTipo.NE;
                campo[4].sCampo = "IDCAJERO";
                campo[4].sValor = modulo.Cajero;

                campo[5] = new cCampo();
                campo[5].iTipo = eTipo.FD;
                campo[5].sCampo = "FECHALOCAL";
                campo[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                campo[6] = new cCampo();
                campo[6].iTipo = eTipo.HR;
                campo[6].sCampo = "HORALOCAL";
                campo[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                campo[7] = new cCampo();
                campo[7].iTipo = eTipo.NE;
                campo[7].sCampo = "TRANSACCION";
                campo[7].sValor = 1;

                campo[8] = new cCampo();
                campo[8].iTipo = eTipo.AN;
                campo[8].sCampo = "SKU";
                campo[8].sValor = modulo.SKU;

                if (referencia != string.Empty)
                {
                    campo[9] = new cCampo();
                    campo[9].sCampo = "REFERENCIA";
                    campo[9].sValor = Encriptacion.PXEncryptFX(referencia, modulo.EncriptionKey);
                    campo[9].bEncriptado = true;
                    campo[9].iTipo = eTipo.AN;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error dentro de la parametrización. {ex}");
            }

            try
            {
                if (modulo.Usuario == string.Empty || modulo.Password == string.Empty)
                {
                    return BadRequest("Imposible conectar al WS porque no hay credenciales");
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = modulo.Usuario;
                    wservice.ClientCredentials.UserName.Password = modulo.Password;
                }

                var response = wservice.InfoAsync(campo).Result;

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
                                return Ok($"Error {codeResponse}: {codeDescription}");
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
                        telReference = response[1].sValor.ToString();

                        try
                        {
                            modulo.ConfirmacionTel = Encriptacion.PXDecryptFX(telReference, modulo.EncriptionKey);
                        }
                        catch (Exception)
                        {
                            modulo.ConfirmacionTel = string.Empty;
                            return NotFound(modulo.ConfirmacionTel.ToString());
                        }
                    }

                    var list = new List<object>();
                    foreach (cCampo wsCampo in response)
                    {
                        if (wsCampo.bEncriptado == true)
                        {
                            wsCampo.sValor = Encriptacion.PXDecryptFX(wsCampo.sValor.ToString(), modulo.EncriptionKey);
                        }
                        list.Add(wsCampo);
                    }

                    (modulo.NoTicket++).ToString();
                    txtNoTransaccion = modulo.NoTicket.ToString();

                    var jsonResponse = JsonConvert.SerializeObject(list);

                    //return Ok(list);

                    return Ok($"La cantidad de elementos devueltos son: {count} \n\nEl JSON:\n\n{jsonResponse}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            return Ok("Final");
        }
        
        // POST: api/Diestel
        [HttpPost]
        public IActionResult PayService(/*[FromBody]string value*/)
        {
            var credentials = _context.conexion_Configs.Find(1);

            string telReference;

            cCampo [] campo = new cCampo[13];

            var modulo = new ModuleTDV();

            modulo.Grupo = 7;
            modulo.Cadena = 1;
            modulo.Tienda = 1;
            modulo.POS = 1;
            modulo.Cajero = 1;

            modulo.Usuario = credentials.Usr;
            modulo.Password = credentials.Pwd;

            //8469760000187
            //8469761001749

            modulo.SKU = "8469760000187";
            //modulo.SKU = SKU;
            var referencia = "8661239618";

            modulo.EncriptionKey = credentials.CrypKey;

            modulo.TokenValor = "1020304050";

            try
            {
                campo[0] = new cCampo();
                campo[0].iTipo = eTipo.NE;
                campo[0].sCampo = "IDGRUPO";
                campo[0].sValor = modulo.Grupo;

                campo[1] = new cCampo();
                campo[1].iTipo = eTipo.NE;
                campo[1].sCampo = "IDCADENA";
                campo[1].sValor = modulo.Cadena;

                campo[2] = new cCampo();
                campo[2].iTipo = eTipo.NE;
                campo[2].sCampo = "IDTIENDA";
                campo[2].sValor = modulo.Tienda;

                campo[3] = new cCampo();
                campo[3].iTipo = eTipo.NE;
                campo[3].sCampo = "IDPOS";
                campo[3].sValor = modulo.POS;

                campo[4] = new cCampo();
                campo[4].iTipo = eTipo.NE;
                campo[4].sCampo = "IDCAJERO";
                campo[4].sValor = modulo.Cajero;

                campo[5] = new cCampo();
                campo[5].iTipo = eTipo.FD;
                campo[5].sCampo = "FECHALOCAL";
                campo[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                campo[6] = new cCampo();
                campo[6].iTipo = eTipo.HR;
                campo[6].sCampo = "HORALOCAL";
                campo[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                campo[7] = new cCampo();
                campo[7].iTipo = eTipo.FD;
                campo[7].sCampo = "FECHACONTABLE";
                campo[7].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                campo[8] = new cCampo();
                campo[8].iTipo = eTipo.NE;
                campo[8].sCampo = "TRANSACCION";
                campo[8].sValor = 1;

                /* Actualizar el ticket de la base de datos */

                campo[9] = new cCampo();
                campo[9].iTipo = eTipo.AN;
                campo[9].sCampo = "SKU";
                campo[9].sValor = modulo.SKU;

                campo[10] = new cCampo();
                campo[10].sCampo = "TIPOPAGO";
                campo[10].iTipo = eTipo.AN;
                campo[10].sValor = "EFE";

                campo[11] = new cCampo();
                campo[11].sCampo = "REFERENCIA";
                campo[11].sValor = "9A4E5ADBAE1E3E0DBA9A83"; //Encriptacion.PXEncryptFX(referencia, modulo.EncriptionKey);
                campo[11].bEncriptado = true;
                //campo[11].iTipo = eTipo.AN;

                campo[12] = new cCampo();
                campo[12].sCampo = "MONTO";
                campo[12].iTipo = eTipo.NE;
                campo[12].sValor = 100.00;

                try
                {
                    if (modulo.Usuario == string.Empty || modulo.Password == string.Empty)
                    {
                        return BadRequest("Imposible conectar al WS porque no hay credenciales");
                    }
                    else
                    {
                        wservice.ClientCredentials.UserName.UserName = modulo.Usuario;
                        wservice.ClientCredentials.UserName.Password = modulo.Password;
                    }

                    var response = wservice.EjecutaAsync(campo).Result;
                    //return Ok(response);

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
                                    return Ok($"Error {codeResponse}: {codeDescription}");
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
                        // Validacion de la referrencia, confirmacion
                        if (response[1].iLongitud.ToString() == "10")
                        {
                            modulo.Confirmacion = true;
                            telReference = response[1].sValor.ToString();

                            try
                            {
                                modulo.ConfirmacionTel = Encriptacion.PXDecryptFX(telReference, modulo.EncriptionKey);
                            }
                            catch (Exception)
                            {
                                return NotFound(modulo.ConfirmacionTel.ToString());
                            }
                        }

                        return Ok(response);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            return NotFound();
            //var MyTX = new TXDiestel();
            //MyTX.setSKU(modulo.SKU.Trim());
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
