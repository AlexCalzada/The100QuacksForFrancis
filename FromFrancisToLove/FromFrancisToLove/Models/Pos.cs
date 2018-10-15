using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("POS", Schema = "HouseOfCards")]
    public class Pos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PosId { get; set; }

        public string Descripcion { get; set; }

        public int estatus { get; set; }
    }
}
