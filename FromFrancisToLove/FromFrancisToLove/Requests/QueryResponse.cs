using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Requests
{
    public class QueryResponse
    {
        public int ID_GRP { get; set; }
        public int ID_CHAIN { get; set; }
        public int ID_MERCHANT { get; set; }
        public int ID_POS { get; set; }
        public string DateTime { get; set; }
        public string PhoneNumber { get; set; }
        public int TransNumber { get; set; }
        public string Brand { get; set; }
        public string Instr1 { get; set; }
        public string Instr2 { get; set; }
        public int AutoNo { get; set; }
        public int ResponseCode { get; set; }
        public string DescripcionCode { get; set; }
    }
}
