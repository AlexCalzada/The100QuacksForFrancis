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

    }










    public class XmlPrueba
    {
        public string Name { get; set; }

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

        public Employee() { }

        public Employee(string name)
        {
            Name = name;
        }
    }

    public class DataEmployee
    {
        public string Age { get; set; }
        public string Dato { get; set; }


        public DataEmployee() { }

        public DataEmployee(string name)
        {
            Age = Age;
            Dato = Dato;
        }

       
        
    }


    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }


    }
}
