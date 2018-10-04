using Diestel;
using FromFrancisToLove.Data;
using FromFrancisToLove.Diestel.Clases;
using FromFrancisToLove.Requests.ModuleDiestel;
using FromFrancisToLove.ServiceInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel
{
    public class TXDiestel
    {
        private  ArrayList alRequest = new ArrayList();
        private  ArrayList alResponse = new ArrayList();
        private string SKU;
        private readonly HouseOfCards_Context _context;
        private readonly PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);
        private ServiceInformation dataInfo = new ServiceInformation();

        protected string usr;
        protected string pwd;
        protected string encrypt;
        protected string numAuto;
        protected string referencia;
        protected string sku;

        public TXDiestel() { }

        public TXDiestel(string _sku, string _referencia, string _numAutorizacion, string _usr, string _pwd, string _encryptKey)
        {
            usr = _usr;
            pwd = _pwd;
            encrypt = _encryptKey;
            numAuto = _numAutorizacion;
            referencia = _referencia;
            sku = _sku;
        }

        public void Init()
        {
            SKU = string.Empty;
            alRequest.Clear();
            alResponse.Clear();
        }

        public void setSKU(string sSKU)
        {
            SKU = sSKU;
        }

        public void AddToRequest(DiestelField NewField)
        {
            alRequest.Add(NewField);
        }

        public void AddToResponse(DiestelField NewField)
        {
            alResponse.Add(NewField);
        }

        public bool InsertRequestFields(DiestelField diestelField)
        {
            try
            {
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool DeleteRequestFields()
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public bool DeleteResponseFields()
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public bool UpdateFields()
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        //Proceso para la cancelacion del Pago
        public void ExecuteReverseProcess(string CodigoRespuesta, string Descripcion, string Tx)
        {
            try
            {
                var reversosLog = new CancelLog("LOG_Reversos_", "txt", @"C:\ApiService\Client\Reversos", LOGFrecuency.Daily);
                reversosLog.WriteTXData(CodigoRespuesta, Descripcion, Tx);

                var ts = new ThreadStart(Reversas);
                var hilo = new Thread(ts);
                hilo.Start();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Reversas()
        {
            
            //var wsClient = _context.conexion_Configs.Find(1);

            cCampo[] requestReversa = new cCampo[12];

            try
            {
                requestReversa[0] = new cCampo();
                requestReversa[0].iTipo = eTipo.NE;
                requestReversa[0].sCampo = "IDGRUPO";
                requestReversa[0].sValor = dataInfo.Grupo;

                requestReversa[1] = new cCampo();
                requestReversa[1].iTipo = eTipo.NE;
                requestReversa[1].sCampo = "IDCADENA";
                requestReversa[1].sValor = dataInfo.Cadena;

                requestReversa[2] = new cCampo();
                requestReversa[2].iTipo = eTipo.NE;
                requestReversa[2].sCampo = "IDTIENDA";
                requestReversa[2].sValor = dataInfo.Tienda;

                requestReversa[3] = new cCampo();
                requestReversa[3].iTipo = eTipo.NE;
                requestReversa[3].sCampo = "IDPOS";
                requestReversa[3].sValor = dataInfo.POS;

                requestReversa[4] = new cCampo();
                requestReversa[4].iTipo = eTipo.NE;
                requestReversa[4].sCampo = "IDCAJERO";
                requestReversa[4].sValor = dataInfo.Cajero;

                requestReversa[5] = new cCampo();
                requestReversa[5].iTipo = eTipo.FD;
                requestReversa[5].sCampo = "FECHALOCAL";
                requestReversa[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestReversa[6] = new cCampo();
                requestReversa[6].iTipo = eTipo.HR;
                requestReversa[6].sCampo = "HORALOCAL";
                requestReversa[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                requestReversa[7] = new cCampo();
                requestReversa[7].iTipo = eTipo.NE;
                requestReversa[7].sCampo = "TRANSACCION";
                requestReversa[7].sValor = 1;

                requestReversa[8] = new cCampo();
                requestReversa[8].iTipo = eTipo.AN;
                requestReversa[8].sCampo = "SKU";
                requestReversa[8].sValor = sku;

                requestReversa[9] = new cCampo();
                requestReversa[9].sCampo = "FECHACONTABLE";
                requestReversa[9].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestReversa[10] = new cCampo();
                requestReversa[10].sCampo = "AUTORIZACION";
                requestReversa[10].sValor = numAuto;

                if (dataInfo.Reference != string.Empty)
                {
                    requestReversa[11] = new cCampo();
                    requestReversa[11].sCampo = "REFERENCIA";
                    requestReversa[11].sValor = Encriptacion.PXEncryptFX(referencia, encrypt);
                    requestReversa[11].bEncriptado = true;
                    requestReversa[11].iTipo = eTipo.AN;
                }

                //TOKEN

                bool bStop = false;
                int intento = 1;
                var logReversos = new CancelLog("LOG_Reversos_", "txt", @"C:\ApiService\Client\Reversos", LOGFrecuency.Daily);

                wservice.ClientCredentials.UserName.UserName = usr;
                wservice.ClientCredentials.UserName.Password = pwd;

                while (!bStop)
                {
                    try
                    {
                        var response = wservice.ReversaAsync(requestReversa).Result;
                        try
                        {
                            if (response[0].sCampo == "CODIGORESPUESTA" && response[0].sValor.ToString() == "0")
                            {
                                logReversos.WriteCancelLog(requestReversa, intento, response[0].sValor.ToString(), response[1].sValor.ToString());
                                bStop = true;
                            }
                            else
                            {
                                logReversos.WriteCancelLog(requestReversa, intento, response[0].sValor.ToString(), response[1].sValor.ToString());

                                if (intento == 3)
                                {
                                    bStop = true;
                                }
                                else
                                {
                                    intento++;
                                }
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                    catch (Exception)
                    {
                        logReversos.WriteCancelLog(requestReversa, intento, "8", "TERMINO EL TIEMPO DE ESPERA DEL INTENTO DE REVERSA");
                        if (intento == 3)
                        {
                            bStop = true;
                        }
                        else
                        {
                            intento++;
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    public class DiestelField
    {
        public string DataType { get; set; }

        public int DataLength { get; set; }

        public string Name { get; set; }

        public DiestelField() { }

        public DiestelField(string DataType, int DataLength, string ProdName)
        {
            this.DataType = DataType;
            this.DataLength = DataLength;
            this.Name = ProdName;
        }
    }
}
