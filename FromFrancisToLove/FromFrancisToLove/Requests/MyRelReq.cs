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
        public int ID_GRP { get; set; }
        [XmlElement("ID_CHAIN")]
        public int ID_CHAIN { get; set; }
        [XmlElement("ID_MERCHANT")]
        public int ID_MERCHANT { get; set; }
        [XmlElement("ID_POS")]
        public int ID_POS { get; set; }
        [XmlElement("DateTime")]
        public string DateTime { get; set; }
        [XmlElement("SKU")]
        public string SKU { get; set; }
        [XmlElement("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [XmlElement("TransNumber")]
        public int TransNumber { get; set; }
        [XmlElement("TC")]
        public int TC { get; set; }
    }

    [XmlRoot("QueryRequest")]
    public class QueryRequest : MyRelReq { }

    [XmlRoot("ReloadRequest")]
    public class ReloadRequest : MyRelReq { }

    [XmlRoot("DataRequest")]
    public class DataRequest : MyRelReq
    {
        [XmlElement("ID_Product")]
        public int ID_Product { get; set; }
    }

    [XmlRoot("DataQueryRequest")]
    public class DataQueryRequest : DataRequest
    {
        [XmlElement("ID_COUNTRY")]
        public int ID_COUNTRY { get; set; }
    }



}

