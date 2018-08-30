using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("Producto", Schema = "HouseOfCards")]
    public class Productos
    {
        public int SKU { get; set; }
        public string Marca { get; set; }
        public int Monto { get; set; }
        public int Vigencia { get; set; }
    }
}
