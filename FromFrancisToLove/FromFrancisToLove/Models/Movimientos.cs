using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("MOVIMIENTO", Schema = "HouseOfCards")]
    public class Movimientos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int MovimientoID { get; set; }
        public int CadenaId { get; set; }

        public int  TiendaId { get; set; }
        public int PosId { get; set; }
        public int CajeroId { get; set; }
        public DateTime Fecha_Local { get; set; }
        public DateTime Hora_Local { get; set; }
        public DateTime Fecha_Contable { get; set; }
        public int Transaccion { get; set; }
        public string SKU { get; set; }
        public string Referencia_1 { get; set; }
        public string Referencia_2 { get; set; }
        public decimal Monto { get; set; }
        public int DV { get; set; }
        public decimal Comision { get; set; }
        public string Token { get; set; }
        public string Autorizacion { get; set; }

        public string Serie { get; set; }



    }
}
