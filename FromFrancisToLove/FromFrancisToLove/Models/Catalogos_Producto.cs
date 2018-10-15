using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("Catalogo_Producto", Schema = "HouseOfCards")]
    public class Catalogos_Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SKU { get; set; }
        public int CONFIGID { get; set; }
        public string IDProduct { get; set; }
        public string Monto { get; set; }
        public string Marca { get; set; }
    }
}
