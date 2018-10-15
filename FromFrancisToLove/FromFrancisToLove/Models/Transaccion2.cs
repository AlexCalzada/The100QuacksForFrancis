using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Requests
{
    [Table("Transaccion2", Schema = "HouseOfCards")]
    public class Transaccion2
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IDTransaccion { get; set; }
        public int ID_GRP { get; set; }
        public int ID_CHAIN { get; set; }
        public int ID_MERCHANT { get; set; }
        public int ID_POS { get; set; }
        public int TransNumber { get; set; }
        public int TC { get; set; }
        public string ID_Product { get; set; }
        public int ID_COUNTRY { get; set; }
        public string Brand { get; set; }
        public string Instr1 { get; set; }
        public string Instr2 { get; set; }
        public string ResponseCode { get; set; }
        public string DescripcionCode { get; set; }
    
        public DateTime Fecha { get; set; }
        public string SKU { get; set; }
        public int AutoNo { get; set; }
        public string Referencia { get; set; }
        public decimal Monto { get; set; }
        public decimal Comision { get; set; }
        public int ID_CONFIG { get; set; }
        public int ID_CAJA { get; set; }
        public long NoTransaccion { get; set; }

    }
}
