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
        public string Nombre { get; set; }
        public string Tipo { get; set; }

        public int ConfigId { get; set; }

        //[ForeignKey("ConfigId")]
        //public virtual Conexion_Config User { get; set; }
    }
}
