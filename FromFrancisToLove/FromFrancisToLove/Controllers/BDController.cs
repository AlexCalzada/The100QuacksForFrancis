using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using FromFrancisToLove.Data;


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


