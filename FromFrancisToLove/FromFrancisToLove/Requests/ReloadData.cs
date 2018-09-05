using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FromFrancisToLove.Requests
{
    public class ReloadData
    {
        public int ID_GRP { get; set; }
        public int ID_CHAIN { get; set; }
        public int ID_MERCHANT { get; set; }
        public int ID_POS { get; set; }
        public string DateTime { get; set; }
        //public string SKU { get; set; }
        public string PhoneNumber { get; set; }
        public int TransNumber { get; set; }
        public string ID_Product { get; set; }
        //public int ID_COUNTRY { get; set; }
        //public int TC { get; set; }
        public string Brand { get; set; }
        public string Instr1 { get; set; }
        public string Instr2 { get; set; }
        public int AutoNo { get; set; }
        public int ResponseCode { get; set; }
        public string DescripcionCode { get; set; }
        public int Monto { get; set; }
    }
}
