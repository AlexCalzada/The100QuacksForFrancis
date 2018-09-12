using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Requests.ModuleDiestel
{
    public class ModuleTDV
    {
        private int iID;
        private string sSKU;
        private string sUser;
        private string sPwd;
        private string sURL;
        private string sURLA;
        private int iGrupo;
        private int iCadena;
        private int iTienda;
        private int iPos;
        private int iCajero;
        private long iTransaccion;
        private int iTimeout;
        private bool schkURL;
        private bool sToken;
        private string vToken;
        private string sRef;
        private string sPath;
        private string sConfTel;
        private string sConfTel2;
        private bool Conf;
        private string sType;
        private bool bNTLM;
        private string EncrypKey;

        public int ID
        {
            get { return iID; }
            set { iID = value; }
        }

        public string EncriptionKey
        {
            get { return EncrypKey; }
            set { EncrypKey = value; }
        }

        public bool AutenticacionNTLM
        {
            get { return bNTLM; }
            set { bNTLM = value; }
        }

        public string TipoProducto
        {
            get { return sType; }
            set { sType = value; }
        }

        public bool Confirmacion
        {
            get
            {
                if (sType.Trim().Equals("TAE"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set { Conf = value; }
        }

        public string ConfirmacionTel2
        {
            get { return sConfTel2; }
            set { sConfTel2 = value; }
        }

        public string ConfirmacionTel
        {
            get { return sConfTel; }
            set { sConfTel = value; }
        }

        public string PathDB
        {
            get { return sPath; }
            set { sPath = value; }
        }

        public string TokenValor
        {
            get { return vToken; }
            set { vToken = value; }
        }

        public bool Token
        {
            get { return sToken; }
            set { sToken = value; }
        }

        public string SKU
        {
            get { return sSKU; }
            set { sSKU = value; }
        }

        public string Usuario
        {
            get { return sUser; }
            set { sUser = value; }
        }

        public string Password
        {
            get { return sPwd; }
            set { sPwd = value; }
        }

        public string URL
        {
            get { return sURL; }
            set { sURL = value; }
        }

        public int Grupo
        {
            get { return iGrupo; }
            set { iGrupo = value; }
        }

        public int Cadena
        {
            get { return iCadena; }
            set { iCadena = value; }
        }

        public int Tienda
        {
            get { return iTienda; }
            set { iTienda = value; }
        }

        public int POS
        {
            get { return iPos; }
            set { iPos = value; }
        }

        public int Cajero
        {
            get { return iCajero; }
            set { iCajero = value; }
        }

        public long NoTicket
        {
            get { return iTransaccion; }
            set { iTransaccion = value; }
        }

        public long NextTicket
        {
            get
            {
                iTransaccion++;
                return (long)iTransaccion;
            }
        }

        public int TimeOut
        {
            get { return iTimeout; }
            set { iTimeout = value; }
        }

        public string Referencia
        {
            get { return sRef; }
            set { sRef = value; }
        }
    }
}
