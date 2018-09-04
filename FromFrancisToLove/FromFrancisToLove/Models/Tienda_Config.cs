using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("")]
    public class Tienda_Config
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TiendaId { get; set; }
        public  int WSCadenaId { get; set; }
        public int WSTieandaId { get; set; }
        public int ConfigId { get; set; }

        [ForeignKey("ConfigId")]
        public virtual Conexion_Config User { get; set; }

    }
}
