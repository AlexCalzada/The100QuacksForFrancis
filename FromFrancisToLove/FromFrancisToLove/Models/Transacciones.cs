using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("Transacciones", Schema = "HouseOfCards")]
    public class Transacciones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransaccionId { get; set; }
        public int WSProveedorId { get; set; }
        public int UserId { get; set; }
        public string SKU { get; set; }
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; }
        public decimal Monto { get; set; }
        public decimal Comision_FF { get; set; }
        public decimal Comision_WS { get; set; }
        public string Pago { get; set; }
        public string sConsulta { get; set; }
        public string sCancelacion { get; set; }
        public int Status_Interno { get; set; }
        public int Status_Inicial { get; set; }
        public int Status_CodeResponse { get; set; }
        public int Estatus { get; set; }
    }
}
