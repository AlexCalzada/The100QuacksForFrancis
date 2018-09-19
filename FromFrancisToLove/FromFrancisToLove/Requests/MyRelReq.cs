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
       
        public string ID_GRP { get; set; }
       
        [XmlElement("ID_CHAIN")]
        public string ID_CHAIN{get;set;}

        [XmlElement("ID_MERCHANT")]
        public string ID_MERCHANT { get; set; }
        [XmlElement("ID_POS")]
        public string ID_POS { get; set; }
        [XmlElement("DateTime")]
        public string DateTime { get; set; }
        [XmlElement("SKU")]
        public string SKU { get; set; }
        [XmlElement("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [XmlElement("TransNumber")]
        public string TransNumber { get; set; }
        [XmlElement("TC")]
        public string TC { get; set; }


    }

    //
    [XmlRoot("QueryRequest")]
    public class QueryRequest : MyRelReq { }

    [XmlRoot("ReloadRequest")]
    public class ReloadRequest : MyRelReq { }

    [XmlRoot("DataRequest")]
    public class DataRequest : MyRelReq
    {
        [XmlElement("ID_Product")]
        public string ID_Product { get; set; }
    }

    [XmlRoot("DataQueryRequest")]
    public class DataQueryRequest : DataRequest
    {
        [XmlElement("ID_COUNTRY")]
        public string ID_COUNTRY { get; set; }
    }


    [XmlRoot("ReloadResponse")]
    public class ReloadResponse : MyRelReq
    {
        public string Brand { get; set; }
        public string Instr1 { get; set; }
        public string Instr2 { get; set; }

        public string AutoNo { get; set; }

        public string ResponseCode { get; set; }

        public string DescripcionCode { get; set; }

        public string ID_COUNTRY { get; set; }
    }


}

