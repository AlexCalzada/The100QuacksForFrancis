using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FromFrancisToLove.Requests
{
    public class MyRelReq
    {

        [XmlElement("ID_GRP")]

        public int ID_GRP
        {

            get { return 7; }
            set { }

        }
        [XmlElement("ID_CHAIN")]
        public int ID_CHAIN
        {
            get { return 1; }
            set { }
        }
        [XmlElement("ID_MERCHANT")]
        public int ID_MERCHANT
        {
            get { return 1; }
            set { }
        }
        [XmlElement("ID_POS")]
        public int ID_POS
        {
            get { return 1; }
            set { }
        }
        [XmlElement("DateTime")]
        public DateTime Datetime
        {
            get
            {
                // return "17/06/2006 21:00:12";
                return DateTime.Now;
            }
            set { }

        }
        [XmlElement("SKU")]
        public string SKU { get; set; }
        [XmlElement("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [XmlElement("TransNumber")]
        public int TransNumber { get { return 1020; } set { } }
        [XmlElement("TC")]
        public int TC { get { return 0; } set { } }

        [XmlElement("ID_Product")]
        public string ID_Product { get; set; }

        [XmlElement("ID_COUNTRY")]
        public int ID_COUNTRY { get; set; }

        public string Brand { get; set; }
        public string Instr1 { get; set; }
        public string Instr2 { get; set; }
        public int AutoNo { get; set; }
        public string ResponseCode { get; set; }
        public string DescripcionCode { get; set; }
        public string Monto { get; set; }

    }
}

