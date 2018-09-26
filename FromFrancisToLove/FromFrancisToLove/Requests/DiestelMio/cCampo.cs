using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FromFrancisToLove.Requests.DiestelMio
{
    public class cCampo
    {
        public string sCampo;
        public eTipo iTipo;
        public int iLongitud;
        public int iClase;
        public object sValor;
        public bool bEncriptado = false;

        public cCampo() { }

        public cCampo(string _sCampo, object _sValor)
        {
            sCampo = _sCampo;
            sValor = _sValor;
        }

        public cCampo(string _sCampo, object _sValor, bool _bEncriptado)
        {
            sCampo = _sCampo;
            sValor = _sValor;
            bEncriptado = _bEncriptado;
        }

        public cCampo(string _sCampo, eTipo _iTipo, int _iLongitud, int _iClase, object _sValor, bool _bEncriptado)
        {
            sCampo = _sCampo;
            iTipo = _iTipo;
            iLongitud = _iLongitud;
            iClase = _iClase;
            sValor = _sValor;
            bEncriptado = _bEncriptado;
        }
    }

    public enum eTipo
    {
        [Description("Indica un valor Alfanumerico 'A,B,C,1,2,3'")]
        AN = 0,

        [Description("Indica un valor Numerico Entero '1, 2, 3'")]
        NE = 1,

        [Description("Indica un valor Numerico Moneda '0.00'")]
        NM = 2,

        [Description("Indica un valor de Fecha 'dd/MM/yyyy'")]
        FD = 3,

        [Description("Indica un valor de Hora 'HH:mm:ss'")]
        HR = 4,

        [Description("Indica un valor Numerico Decimal '0.00'")]
        ND = 5
    }
}