﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diestel;
using FromFrancisToLove.Data;
using FromFrancisToLove.Requests.ModuleDiestel;
using FromFrancisToLove.ServiceInfo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/Services")]
    public class ServicesController : Controller
    {
        private readonly HouseOfCards_Context _context;
        private readonly PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);
        private ServiceInformation dataInfo = new ServiceInformation();

        public ServicesController(HouseOfCards_Context context)
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
                }
            }
            catch (Exception)
            {
                dataInfo.ProviderId = -1;
            }
        }

        // GET: api/Services
        [HttpGet("GetSKUs")]
        public IActionResult Get()
        {
            return Ok(_context.catalogos_Productos);
        }

        [HttpGet("RequestService")]
        public IActionResult RequestService([FromQuery]string SKU, [FromQuery]string Reference)
        {
            /* 
               Se obtienen los datos del servicio a utilizar
               de acuerdo al SKU y Referencia recibidos
            */
            GetServiceData(SKU, Reference);

            string jsonResult = "Default Message";

            // Diestel
            if (dataInfo.ProviderId == (int)Provider.Diestel)
            {
                cCampo[] campo = new cCampo[10];

                //8469760000187
                //8469761001749

                //modulo.TokenValor = "1020304050";

                if (dataInfo.SKU == string.Empty)
                {
                    return BadRequest($"No se ha especificado el servicio a pagar");
                }

                try
                {
                    campo[0] = new cCampo();
                    campo[0].iTipo = eTipo.NE;
                    campo[0].sCampo = "IDGRUPO";
                    campo[0].sValor = dataInfo.Grupo;

                    campo[1] = new cCampo();
                    campo[1].iTipo = eTipo.NE;
                    campo[1].sCampo = "IDCADENA";
                    campo[1].sValor = dataInfo.Cadena;

                    campo[2] = new cCampo();
                    campo[2].iTipo = eTipo.NE;
                    campo[2].sCampo = "IDTIENDA";
                    campo[2].sValor = dataInfo.Tienda;

                    campo[3] = new cCampo();
                    campo[3].iTipo = eTipo.NE;
                    campo[3].sCampo = "IDPOS";
                    campo[3].sValor = dataInfo.POS;

                    campo[4] = new cCampo();
                    campo[4].iTipo = eTipo.NE;
                    campo[4].sCampo = "IDCAJERO";
                    campo[4].sValor = dataInfo.Cajero;

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
                    campo[8].sValor = dataInfo.SKU;

                    if (dataInfo.Reference != string.Empty)
                    {
                        campo[9] = new cCampo();
                        campo[9].sCampo = "REFERENCIA";
                        campo[9].sValor = Encriptacion.PXEncryptFX(dataInfo.Reference, dataInfo.EncryptionKey);
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
                    if (dataInfo.User == string.Empty || dataInfo.Password == string.Empty)
                    {
                        return BadRequest("Imposible conectar al WS porque no hay credenciales");
                    }
                    else
                    {
                        wservice.ClientCredentials.UserName.UserName = dataInfo.User;
                        wservice.ClientCredentials.UserName.Password = dataInfo.Password;
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
                            if (wsCampo.bEncriptado == true)

                            {
                                wsCampo.sValor = Encriptacion.PXDecryptFX(wsCampo.sValor.ToString(), dataInfo.EncryptionKey);
                            }
                            list.Add(wsCampo);
                        }

                        //(modulo.NoTicket++).ToString();
                        //txtNoTransaccion = modulo.NoTicket.ToString();

                        var jsonResponse = JsonConvert.SerializeObject(list);
                        jsonResult = jsonResponse;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

                //return Ok("Final");
            }
            
            // Tadenor
            else if(dataInfo.ProviderId == (int)Provider.Tadenor)
            {

            }

            else if (dataInfo.ProviderId < 0)
            {
                jsonResult = "[{'Error':'No se encontró el proveedor solicitado'}]";
            }

            return Ok(jsonResult);
        }


        // PUT: api/Services/5
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
