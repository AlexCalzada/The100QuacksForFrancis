using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FromFrancisToLove.Requests
{
    public class ReloadRequest
    {
        public int ID_GRP { get; set; }
        public int ID_CHAIN { get; set; }
        public int ID_MERCHANT { get; set; }
        public int ID_POS { get; set; }
        public string DateTime { get; set; }
        public string SKU { get; set; }
        public string PhoneNumber { get; set; }
        public int TransNumber { get; set; }
        public int ID_COUNTRY { get; set; }
        public int TC { get; set; }
        
    }
}
