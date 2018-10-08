using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("LOGTRANSACCIONAL", Schema = "HouseOfCards")]
    public class LogTransaccional
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }
        public int N_Transaccional { get; set; }
        public int N_Autorizacion { get; set; }
        public decimal Monto { get; set; }
        public string Telefono { get; set; }
        public string Fecha { get; set; }
        public int N_Oficina { get; set; }
        public int Clave_Cajero { get; set; }
        public int Estatus { get; set; }

    }
}
