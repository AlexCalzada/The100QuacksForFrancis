
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace FromFrancisToLove.Requests
{
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope_SD
    {
        [XmlElement("Body")]
        public Body_SD body{ get;set; }
    }

    [XmlRoot("Body")]
    public  class Body_SD
    {
        [XmlElement("SaldoDisponible", Namespace = "http://www.pagoexpress.com.mx/ServicePX")]
        public Campos_SD SaldoDisponible { get; set; }
    }

    [XmlRoot("SaldoDisponible")]
    public class Campos_SD
    {
        [XmlElement("lGrupo")]
        public int lGrupo{get; set;}

        [XmlElement("lCadena")]
        public int lCadena { get; set; }

        [XmlElement("lTienda")]
        public int lTienda { get; set; }

    }
}
