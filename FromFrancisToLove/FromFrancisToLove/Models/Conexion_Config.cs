using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    [Table("CONEXION_CONFIG", Schema = "HouseOfCards")]
    public class Conexion_Config
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigID { get; set; }
        public string Url { get; set; }
        public string Usr { get; set; }
        public string Pwd { get; set; }
        public string CrypKey { get; set; }
        public int  WSGroupId { get; set; }

    }
}
