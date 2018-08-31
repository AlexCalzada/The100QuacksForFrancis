﻿using System;
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
  /*FECHA_LOCAL DATE,
  HORA_LOCAL TIME,
  FECHA_CONTABLE DATE,
  TRANSACCION INT(11),
  SKU VARCHAR(13),
  TIPO_PAGO VARCHAR(20),
  REFERENCIA_1 VARCHAR(120),
  REFERENCIA_2 VARCHAR(120),
  MONTO DECIMAL(10,2),
  DV INT(4),
  COMISION DECIMAL(10,2),
  TOKEN VARCHAR(8),
  AUTORIZACION CHAR(20),
  SERIE VARCHAR(19)        
         */
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MovimientoId { get; set; }
        public int GrupoId { get; set; }
        public int CadenaId { get; set; }
        public int TiendaId { get; set; }
        public int PosId { get; set; }
        public int CajeroId { get; set; }
        public string Fecha_Local { get; set; }
        public string Hora_Local { get; set; }
        public string Fecha_Contable { get; set; }
        public int Transaccion { get; set; }
        public string Referencia_1 { get; set; }
        public string Referencia_2 { get; set; }
        public decimal Monto { get; set; }
        public int DV { get; set; }
        public decimal Comision { get; set; }

        public string Token { get; set; }


    }
}
