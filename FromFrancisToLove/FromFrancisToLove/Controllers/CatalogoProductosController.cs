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
        [HttpPost("1")]
        public IActionResult Post()
        {
            try
            {
                //var RelReq = new ReloadData();
                //RelReq.ID_GRP = 7;
                //RelReq.ID_CHAIN = 1;
                //RelReq.ID_MERCHANT = 1;
                //RelReq.ID_POS = 1;
                //RelReq.DateTime = DateTime.Now.ToString();
                ////RelReq.SKU = "7378840101007";
                //RelReq.PhoneNumber = "1020304050";
                //RelReq.TransNumber = 1020;
                //RelReq.ID_Product = "SBH001";
                ////RelReq.ID_COUNTRY = 0484;
                ////RelReq.TC = 0;
                //RelReq.Brand = "TELCEL";
                //RelReq.Instr1 = "MARCA *264";
                //RelReq.Instr2 = "VIGENCIA TIEMPO AIRE 23 DIAS";
                //RelReq.AutoNo = 0;
                //RelReq.ResponseCode = 00;
                //RelReq.DescripcionCode = "Reload Success";
                //RelReq.Monto = 1340;

                var queryReq = new QueryRequest();
                queryReq.ID_GRP = 7;
                queryReq.ID_CHAIN = 1;
                queryReq.ID_MERCHANT = 1;
                queryReq.ID_POS = 1;
                queryReq.DateTime = DateTime.Now.ToString();
                queryReq.SKU = "7378840101007";
                queryReq.PhoneNumber = "8661425585";
                queryReq.TransNumber = 1020;
                //RelReq.ID_Product = "SBH001";
                queryReq.ID_COUNTRY = 0484;
                queryReq.TC = 0;
                //RelReq.Brand = "TELCEL";
                //RelReq.Instr1 = "MARCA *264";
                //RelReq.Instr2 = "VIGENCIA TIEMPO AIRE 23 DIAS";
                //RelReq.AutoNo = 0;
                //RelReq.ResponseCode = 00;
                //RelReq.DescripcionCode = "Reload Success";
                //RelReq.Monto = 1340;

                XmlSerializer xmlSerializer = new XmlSerializer(queryReq.GetType());

                StringWriter sw = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sw);
                xmlSerializer.Serialize(writer, queryReq);
                var xml = sw.ToString();

                
                ServicePXSoapClient client = new ServicePXSoapClient(ServicePXSoapClient.EndpointConfiguration.ServicePXSoap);

                client.ClientCredentials.UserName.UserName = Connected_Services.Tadenor.CredentialsTadenor.Usr;
                client.ClientCredentials.UserName.Password = Connected_Services.Tadenor.CredentialsTadenor.Psw;
                
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                //var x = xml.ToString();

                var recarga = client.getQueryClassAsync(xml.ToString());

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
