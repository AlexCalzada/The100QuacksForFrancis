using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("CAJERO", Schema = "HouseOfCards")]
    public class Cajeros
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CajeroId { get; set; }

        public string Descripcion { get; set; }

        public int estatus { get; set; }
    }
}
