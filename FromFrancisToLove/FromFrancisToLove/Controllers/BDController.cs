using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using FromFrancisToLove.Data;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FromFrancisToLove.Controllers
{
    [Route("PruebaBD")]
    public class BDController : Controller
    {
        private readonly HouseOfCards_Context _context;

        public BDController(HouseOfCards_Context context)
        {          
            _context = context;
        }
    }
}
