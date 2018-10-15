using Diestel;
using FromFrancisToLove.Requests.ModuleDiestel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel.Clases
{
    public class CancelationService
    {
        //Credenciales
        private string User;
        private string Password;
        private string EncryptionKey;

        //Datos requeridos
        private string SKU;           //[x]
        private string Reference;     //[x] _  Dentro del arreglo
        private int NoAuto;           //[x] _  cCampos
        private long Transaction;      //[x]
         

        public CancelationService(string[] data)
        {
            User = data[0];
            Password = data[1];
            EncryptionKey = data[2];
            Transaction = long.Parse(data[3]);
        }

        public (string result, int status) Reverses(List<cCampo> campos)
        {
            foreach (var campo in campos)
            {
                if (campo.sCampo == "SKU")
                {
                    SKU = campo.sValor.ToString();
                }
                if (campo.sCampo == "REFERENCIA")
                {
                    Reference = campo.sValor.ToString();
                }
                if (campo.sCampo == "AUTORIZACION")
                {
                    NoAuto = (int)campo.sValor;
                }
                break;
            }

            PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);

            cCampo[] requestReversa = new cCampo[12];

            string response = "";

            try
            {
                requestReversa[0] = new cCampo();
                requestReversa[0].iTipo = eTipo.NE;
                requestReversa[0].sCampo = "IDGRUPO";
                requestReversa[0].sValor = 7;

                requestReversa[1] = new cCampo();
                requestReversa[1].iTipo = eTipo.NE;
                requestReversa[1].sCampo = "IDCADENA";
                requestReversa[1].sValor = 1;

                requestReversa[2] = new cCampo();
                requestReversa[2].iTipo = eTipo.NE;
                requestReversa[2].sCampo = "IDTIENDA";
                requestReversa[2].sValor = 1;

                requestReversa[3] = new cCampo();
                requestReversa[3].iTipo = eTipo.NE;
                requestReversa[3].sCampo = "IDPOS";
                requestReversa[3].sValor = 1;

                requestReversa[4] = new cCampo();
                requestReversa[4].iTipo = eTipo.NE;
                requestReversa[4].sCampo = "IDCAJERO";
                requestReversa[4].sValor = 1;

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
                requestReversa[7].sValor = Transaction;

                requestReversa[8] = new cCampo();
                requestReversa[8].iTipo = eTipo.AN;
                requestReversa[8].sCampo = "SKU";
                requestReversa[8].sValor = SKU;

                requestReversa[9] = new cCampo();
                requestReversa[9].sCampo = "FECHACONTABLE";
                requestReversa[9].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestReversa[10] = new cCampo();
                requestReversa[10].sCampo = "AUTORIZACION";
                requestReversa[10].sValor = NoAuto;

                if (Reference != string.Empty)
                {
                    requestReversa[11] = new cCampo();
                    requestReversa[11].sCampo = "REFERENCIA";
                    requestReversa[11].sValor = Encriptacion.PXEncryptFX(Reference, EncryptionKey);
                    requestReversa[11].bEncriptado = true;
                    requestReversa[11].iTipo = eTipo.AN;
                }

                //Se verifica que existan las credenciales
                if (User == string.Empty || Password == string.Empty || User == null || Password == null)
                {
                    response = "Imposible conectar al WS porque no hay credenciales";
                    return (response, -1);
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = User;
                    wservice.ClientCredentials.UserName.Password = Password;
                }

                cCampo[] fields = null;

                try
                {
                    var task = Task.Run(() => wservice.ReversaAsync(requestReversa));

                    // Se registra la solicitud de pago enviada en el TXT
                    //LogReversos.WriteTXData("+", "Solicitud de Pago enviada", currentTransaction.ToString());

                    // Se establece el tiempo de espera para la respuesta
                    var timeout = TimeSpan.FromSeconds(45);

                    // Obtendremos: TRUE si la tarea se ejecuto dentro del tiempo establecido
                    //              FALSE si la tarea sobrepaso de ese tiempo
                    var isTaskFinished = task.Wait(timeout);

                    // Se va contabilizando los intentos para obtener respuesta del WS
                    int attempts = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        //Si se obtuvo respuesta se detiene la iteracion y
                        // continuara con la logica establecida
                        if (isTaskFinished)
                        {
                            fields = task.Result;
                            break;
                        }
                        else
                        {
                            //SE incremetnaran los intentos hasta obtener una respuesta
                            attempts++;

                            //Actualizar tabla [Transaccion] con el intento

                            task = Task.Run(() => wservice.ReversaAsync(requestReversa));
                        }
                    }

                    if (fields[0].sCampo == "CODIGORESPUESTA" && fields[0].sValor.ToString() == "0")
                    {
                        //ACTUALIZAR en la BD de la reversa con los intentos
                        response = fields[0].sValor.ToString();
                        
                        return (response, 1);
                    }
                    else
                    {
                        //Isertamos el resultado del WS en BD
                        response = fields[0].sValor.ToString();
                        return (response, 1);
                    }
                }
                catch (System.Net.WebException wex)
                {
                    //Insertar por tiempo de espera BD
                    response = wex.ToString();
                    return (response, 3);
                }

            }
            catch (Exception ex)
            {
                string respuesta = ex.ToString();
                return (respuesta, 3);
            }
        }
    }
}
