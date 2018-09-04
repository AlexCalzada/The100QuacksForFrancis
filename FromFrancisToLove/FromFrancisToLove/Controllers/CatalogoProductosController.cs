using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FromFrancisToLove.Data;

namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/CatalogoProductos")]
    public class CatalogoProductosController : Controller
    {
        private readonly HouseOfCards_Context _context;

        public CatalogoProductosController(HouseOfCards_Context context)
        {
            _context = context;
        }

        // GET: api/CatalogoProductos
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.catalogos_Productos.ToList());
        }

        // GET: api/CatalogoProductos/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/CatalogoProductos
        [HttpPost]
        public void Post([FromBody]string value)
        {
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
