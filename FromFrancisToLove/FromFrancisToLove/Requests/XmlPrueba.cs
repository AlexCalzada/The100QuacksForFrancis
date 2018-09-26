using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FromFrancisToLove.Requests
{
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope_PE
    {
        [XmlElement("Body")]
        public Body_PE Body { get; set; }
    }
    [XmlRoot("Body")]
    public class Body_PE
    {
        [XmlElement("Info", Namespace = "http://www.pagoexpress.com.mx/pxUniversal")]
        public Info_PE Info { get; set; }
    }
    [XmlRoot("Info")]
    public class Info_PE
    {
        [XmlElement("cArrayCampos")]
        public cArrayCampos_PE cArrayCampos { get; set; }



    }

    [XmlRoot("cArrayCampos")]
    public class cArrayCampos_PE
    {
        public List<cCampo_PE> cCampos_PE { get; set; }

        public cArrayCampos_PE()
        {
            cCampos_PE = new List<cCampo_PE>();
        }

    }

    public class cCampo_PE
    {

        public List<cCampo_PE> cCampos_PE { get; set; }


       
        public cCampo_PE() { }



        public string sCampo { get; set; }
        public int iTipo { get; set; }
        public int Longitud { get; set; }
        public int iClase { get; set; }
        public int sValor { get; set; }
        public bool bEncriptado { get; set; }

        public cCampo_PE(string scampo, int itipo, int longitud, int iclase,int svalor,bool bencriptado)
        {
            sCampo = scampo;
            iTipo = itipo;
            Longitud = longitud;
            iClase = iclase;
            sValor = svalor;
            bEncriptado = bencriptado;

        }

    }

    public class Data_cCampo_PE
    {

        public string Dato { get; set; }

        public Data_cCampo_PE() { }

        public Data_cCampo_PE(string scampo, int itipo, int longitud, int iclase, int svalor, bool bencriptado)
        {
            Dato = Dato;
        }
    }




    public class XmlPruebaPadre
    {
        public XmlPrueba xmlPrueba { get; set; }

    }



    public class XmlPrueba
    {
    
        public List<Employee> Employees { get; set; }

        public XmlPrueba()
        {
            Employees = new List<Employee>();
        }
    }

    public class Employee
    {
         public List<DataEmployee> DataEmployee { get; set; }
        

        public string Name { get; set; }

        public string Edad { get; set; }
        public Employee() { }

        public Employee(string name,string edad)
        {
            Name = name;
            Edad = edad;
        }

    }

    public class DataEmployee
    {
  
        public string Dato { get; set; }

        public DataEmployee() { }

        public DataEmployee(string name, string edad)
        {
            Dato = Dato;
        }   
    }



}
