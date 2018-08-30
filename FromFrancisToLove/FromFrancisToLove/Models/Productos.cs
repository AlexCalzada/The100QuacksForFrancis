using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("PRODUCTO", Schema = "HouseOfCards")]
    public class Productos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductoId { get; set; }
        public long SKU { get; set; }
        public string Marca { get; set; }
        public int Monto { get; set; }
        public int Vigencia { get; set; }
    }
}
