using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FromFrancisToLove.Data;
using System.Xml.Serialization;
using FromFrancisToLove.Requests;
using System.IO;
using Tadenor;
using Diestel;
using System.Xml;
using FromFrancisToLove.Requests.ModuleDiestel;
using PXSecurity.Datalogic.Classes;
using FromFrancisToLove.Models;

namespace FromFrancisToLove.Controllers
{
    //[Produces("application/xml")]
    [Route("api/CatalogoProductos")]
    public class CatalogoProductosController : Controller
    {
        private readonly HouseOfCards_Context _context;
        //
        public CatalogoProductosController(HouseOfCards_Context context)
        {
            _context = context;
        }

        // GET: api/CatalogoProductos
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                PxUniversalSoapClient client = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);
                client.ClientCredentials.UserName.UserName = Connected_Services.Diestel.CredentialsDiestel.Usr;
                client.ClientCredentials.UserName.Password = Connected_Services.Diestel.CredentialsDiestel.Psw;
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                cCampo[] cCamp = new cCampo[10];
                var modulo = new ModuleTDV();

                

                modulo.Grupo = 7;
                modulo.Cadena = 1;
                modulo.Tienda = 1;
                modulo.POS = 1;
                modulo.Cajero = 1;
                modulo.SKU = "8469761001749";
                modulo.EncriptionKey = "pxD4t09*09Wm";
                modulo.TokenValor = "1020304050";

                cCamp[0] = new cCampo();
                cCamp[0].sCampo = "IDGRUPO";
                cCamp[0].sValor = modulo.Grupo;

                cCamp[1] = new cCampo();
                cCamp[1].sCampo = "IDCADENA";
                cCamp[1].sValor = modulo.Cadena;

                cCamp[2] = new cCampo();
                cCamp[2].sCampo = "IDTIENDA";
                cCamp[2].sValor = modulo.Tienda;

                cCamp[3] = new cCampo();
                cCamp[3].sCampo = "IDPOS";
                cCamp[3].sValor = modulo.POS;

                cCamp[4] = new cCampo();
                cCamp[4].sCampo = "IDCAJERO";
                cCamp[4].sValor = modulo.Cajero;

                cCamp[5] = new cCampo();
                cCamp[5].sCampo = "FECHALOCAL";
                cCamp[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                cCamp[6] = new cCampo();
                cCamp[6].sCampo = "HORALOCAL";
                cCamp[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                //cCamp[7] = new cCampo();
                //cCamp[7].sCampo = "TRANSACCION";
                //cCamp[7].sValor = modulo.NextTicket;
                //lblNoTX.Text = "No. Transaccion: " & cCamp(7).sValor
                //lblNoTX.Refresh()
                //actualizamos el numero de ticket en la BD
                //UpdateTX(ModPDV.ID, CLng(cCamp(7).sValor))

                cCamp[7] = new cCampo();
                cCamp[7].sCampo = "FECHACONTABLE";
                cCamp[7].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                cCamp[8] = new cCampo();
                cCamp[8].sCampo = "SKU";
                cCamp[8].sValor = modulo.SKU;

                cCamp[9] = new cCampo();
                cCamp[9].sCampo = "REFERENCIA";
                //cCamp[10].sValor = Encriptacion.PXEncryptFX("9A4E5ADBAE1E3E0DBA9A83", modulo.EncriptionKey);
                cCamp[9].sValor = "9A4E5ADBAE1E3E0DBA9A83";
                cCamp[9].bEncriptado = false;

                //cCamp[11] = new cCampo();
                //cCamp[11].sCampo = "TOKEN";
                //cCamp[11].sValor = modulo.TokenValor;
                




                var response = client.InfoAsync(cCamp).Result;

                //var response = _context.catalogos_Productos.Where(n => n.ConfigId == 1);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // GET: api/CatalogoProductos/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(string value)
        {
            if (value != "")
            {
                var response = _context.catalogos_Productos.Select(n => new Catalogos_Producto
                {
                    SKU = n.SKU,
                    Nombre = n.Nombre
                }).Where(n => n.Tipo == value);

                return Ok(response);
            }
            else
            {
                return NotFound();
            }
            
        }
        
        // POST: api/CatalogoProductos
        [HttpPost("1")]
        public IActionResult Post()
        {
            try
            {            

                var query = new ReloadRequest();

                query.ID_GRP = 7+"";
                query.ID_CHAIN = 1+"";
                query.ID_MERCHANT = 1+"";
                query.ID_POS = 1+"";
                query.DateTime = DateTime.Now.ToString();
                query.SKU = "8469760101006";
                query.PhoneNumber = "1020304050";
                query.TransNumber = 1020+"";
                //query.ID_Product = "SBH001";
                //query.ID_COUNTRY = 0;
                query.TC = 0+"";

                //Serialización
                XmlSerializer xmlSerializer = new XmlSerializer(query.GetType());
                StringWriter sw = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sw);
                xmlSerializer.Serialize(writer, query);
                var xml = sw.ToString();

                //Se mandan las credenciales
                ServicePXSoapClient client = new ServicePXSoapClient(ServicePXSoapClient.EndpointConfiguration.ServicePXSoap);

                client.ClientCredentials.UserName.UserName = Connected_Services.Tadenor.CredentialsTadenor.Usr;
                client.ClientCredentials.UserName.Password = Connected_Services.Tadenor.CredentialsTadenor.Psw;
                
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                
                //Se almacena la respuesta del ws
                var response = client.getReloadClassAsync(xml.ToString()).Result;

                // Deserealización
                //var response_des = XmlToObject(response, typeof(ReloadResponse));

                //if (response_des == null)
                //{
                //    //return NotFound();
                //}


                //return Ok(response);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public static object XmlToObject(string xml, Type objectType)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            object obj = null;
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(objectType);
                xmlReader = new XmlTextReader(strReader);
                obj = serializer.Deserialize(xmlReader);
            }
            catch (Exception exp)
            {
                //Handle Exception Code
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }

        // PUT: api/CatalogoProductos/5
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
