using Diestel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.ServiceInfo
{
    public class ServiceInformation
    {
        // Credenciales, identificar el servicio
        public string SKU { get; set; }
        public int ProviderId { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string EncryptionKey { get; set; }
        public string URL { get; set; }

        //Datos fijos para el WS
        public int Grupo { get { return 7; } set { } }
        public int Cadena { get { return 1; } set { } }
        public int Tienda { get { return 1; } set { } }
        public int POS { get { return 1; } set { } }
        public int Cajero { get { return 1; } set { } }

        public long Transaccion { get; set; }

        //Datos Dinamicos
        public string Reference { get; set; }
        public string ReferenceConfirm { get; set; }
        public decimal Comision { get; set; }
        public string Token { get; set; }
        public decimal Monto { get { return 0; } set { } }

        //Dato Temporal
        public long NumeroAutorizacion { get; set; }
        
        //Datos del JSON
        public string jSKU { get; set; }
        public string jReference { get; set; }
        public int jProvider { get; set; }

        //Temporales (Solo de prueba)
        public string TelReference { get; set; }
        public string JSON { get; set; }
    }

    public class JsonRequest
    {
        public string jSKU { get; set; }
        public string jReference { get; set; }
        public int jProvider { get; set; }

        public cCampo campo { get; set; }
    }

    public enum Provider
    {
        Diestel = 1,
        Tadenor = 2
    }
}
