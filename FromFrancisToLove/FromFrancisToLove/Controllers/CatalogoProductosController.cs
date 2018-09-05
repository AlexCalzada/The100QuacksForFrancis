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
            return Ok();
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
            var RelReq = new ReloadRequest();
            RelReq.ID_GRP = 7;
            RelReq.ID_CHAIN = 1;
            RelReq.ID_MERCHANT = 1;
            RelReq.ID_POS = 1;
            RelReq.DateTime = DateTime.Now;
            RelReq.SKU = "8469760101006";
            RelReq.PhoneNumber = 8661625268;
            RelReq.TransNumber = 1020;
            RelReq.TC = 4;

            var xml = new XmlSerializer(RelReq.GetType());
            MemoryStream file = new MemoryStream();
            xml.Serialize(file, RelReq);

            ServicePXSoapClient.EndpointConfiguration endpoint = new ServicePXSoapClient.EndpointConfiguration();
            ServicePXSoapClient client = new ServicePXSoapClient(endpoint);

            client.ClientCredentials.UserName.UserName = Connected_Services.Tadenor.CredentialsTadenor.Usr;
            client.ClientCredentials.UserName.Password = Connected_Services.Tadenor.CredentialsTadenor.Psw;
            client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

            var recarga = client.getReloadClassAsync(file.ToString());

            return Ok($"{recarga.Result}");
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
