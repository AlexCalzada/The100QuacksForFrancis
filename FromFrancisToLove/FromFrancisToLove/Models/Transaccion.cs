using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace FromFrancisToLove.Models
{
    [Table("Transaccion", Schema = "HouseOfCards")]
    public class Transaccion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TxID { get; set; }
        public DateTime FechaTx { get; set; }
        public string Sku { get; set; }
        public long NAutorizacion { get; set; }
        public string Referencia { get; set; }
        public decimal Monto { get; set; }
        public decimal Comision { get; set; }
        public int ConfigID { get; set; }
        public int TiendaID { get; set; }
        public int CajaID { get; set; }
        public long NoTransaccion { get; set; }
    }
}
