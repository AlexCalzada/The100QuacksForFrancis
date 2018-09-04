using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FromFrancisToLove.Models;
using FromFrancisToLove.Data;
using FromFrancisToLove.Connected_Services.Tadenor;
using Tadenor;


namespace FromFrancisToLove.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly HouseOfCards_Context _context;

        public ValuesController(HouseOfCards_Context context)
        {
            _context = context;
        }

        // GET api/values
        [HttpGet]
  
        public IActionResult Get()
        {
            try
            {
                ServicePXSoapClient.EndpointConfiguration endpoint = new ServicePXSoapClient.EndpointConfiguration();
                ServicePXSoapClient client = new ServicePXSoapClient(endpoint, Credentials.Url);

                client.ClientCredentials.UserName.UserName = Credentials.Usr;
                client.ClientCredentials.UserName.Password = Credentials.Psw;
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                var saldo = client.SaldoDisponibleAsync(7, 1, 1).Result;


                return Ok(saldo);
            }
            catch (Exception ex)
            {
                return Ok($"{ex}");
            }
        }

        [HttpGet("0")]
        public IActionResult GetBD()
        {
            return Json( _context.Conexion_Config.ToList());
        }

        [HttpGet("1",Name ="Obtener_Conexion_Config")]
        public IActionResult GetById(int id)
        {

            var item = _context.Conexion_Config.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            try
            {
                ServicePXSoapClient.EndpointConfiguration endpoint = new ServicePXSoapClient.EndpointConfiguration();
                ServicePXSoapClient client = new ServicePXSoapClient(endpoint, item.Url);

                client.ClientCredentials.UserName.UserName = item.Usr;
                client.ClientCredentials.UserName.Password = item.Pwd;
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                var saldo = client.SaldoDisponibleAsync(7, 1, 1).Result;


                return Ok(saldo);
            }
            catch (Exception ex)
            {
                return Ok($"{ex}");
            }
            return Ok(item);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
