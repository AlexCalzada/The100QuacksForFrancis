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

                var campo = new cCampo[1];

                campo[0] = new cCampo();

                campo[0].sCampo = "CODIGORESPUESTA";
                campo[0].iTipo = eTipo.NE;
                campo[0].iLongitud = 4;
                campo[0].iClase = 0;
                campo[0].sValor = 60;
                campo[0].bEncriptado = true;

                var response = client.InfoAsync(campo).Result;

                return Ok(response);
            }
            catch (Exception ex)
            {

                return NotFound(ex);
            }
        }

        // GET: api/CatalogoProductos/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/CatalogoProductos
        [HttpPost]
        public IActionResult Post()
        {
            try
            {
                var RelReq = new ReloadData();
                RelReq.ID_GRP = 7;
                RelReq.ID_CHAIN = 1;
                RelReq.ID_MERCHANT = 1;
                RelReq.ID_POS = 1;
                RelReq.DateTime = DateTime.Now.ToString();
                //RelReq.SKU = "8469760101006";
                RelReq.PhoneNumber = "8661625268";
                RelReq.TransNumber = 1020;
                RelReq.ID_Product = "SBH001";
                //RelReq.ID_COUNTRY = 484;
                //RelReq.TC = 0;
                RelReq.Brand = "TELCEL";
                RelReq.Instr1 = "MARCA *264";
                RelReq.Instr2 = "VIGENCIA TIEMPO AIRE 30 DIAS";
                RelReq.AutoNo = 0;
                RelReq.ResponseCode = 00;
                RelReq.DescripcionCode = "Reload Success";
                RelReq.Monto = 1521;


                XmlSerializer xmlSerializer = new XmlSerializer(RelReq.GetType());

                StringWriter sw = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sw);
                xmlSerializer.Serialize(writer, RelReq);
                var xml = sw.ToString();

                
                ServicePXSoapClient client = new ServicePXSoapClient(ServicePXSoapClient.EndpointConfiguration.ServicePXSoap);

                client.ClientCredentials.UserName.UserName = Connected_Services.Tadenor.CredentialsTadenor.Usr;
                client.ClientCredentials.UserName.Password = Connected_Services.Tadenor.CredentialsTadenor.Psw;
                
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                //var x = xml.ToString();

                var recarga = client.getReloadDataAsync(xml.ToString());

                return Ok(recarga.Result);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
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
