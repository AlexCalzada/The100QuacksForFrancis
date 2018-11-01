using Diestel;
using FromFrancisToLove.Requests.ModuleDiestel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel.Clases
{
    public class RequestActiveService
    {
        private string SKU;
        private string Reference;
        private long   currentTransaction;
        private string User;
        private string Password;
        private string EncryptionKey;

        public RequestActiveService(string[] data)
        {
            SKU = data[0];
            Reference = data[1];
            currentTransaction = long.Parse(data[2]);
            User = data[3];
            Password = data[4];
            EncryptionKey = data[5];
        }

        public (string response, int estatus) RequestService()
        {
            PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);
            
            try
            {
                string jsonResult = "";

                cCampo[] requestInfo = new cCampo[10];

                requestInfo[0] = new cCampo();
                requestInfo[0].iTipo = eTipo.NE;
                requestInfo[0].sCampo = "IDGRUPO";
                requestInfo[0].sValor = 7;

                requestInfo[1] = new cCampo();
                requestInfo[1].iTipo = eTipo.NE;
                requestInfo[1].sCampo = "IDCADENA";
                requestInfo[1].sValor = 1;

                requestInfo[2] = new cCampo();
                requestInfo[2].iTipo = eTipo.NE;
                requestInfo[2].sCampo = "IDTIENDA";
                requestInfo[2].sValor = 1;

                requestInfo[3] = new cCampo();
                requestInfo[3].iTipo = eTipo.NE;
                requestInfo[3].sCampo = "IDPOS";
                requestInfo[3].sValor = 1;

                requestInfo[4] = new cCampo();
                requestInfo[4].iTipo = eTipo.NE;
                requestInfo[4].sCampo = "IDCAJERO";
                requestInfo[4].sValor = 1;

                requestInfo[5] = new cCampo();
                requestInfo[5].iTipo = eTipo.FD;
                requestInfo[5].sCampo = "FECHALOCAL";
                requestInfo[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestInfo[6] = new cCampo();
                requestInfo[6].iTipo = eTipo.HR;
                requestInfo[6].sCampo = "HORALOCAL";
                requestInfo[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                requestInfo[7] = new cCampo();
                requestInfo[7].iTipo = eTipo.NE;
                requestInfo[7].sCampo = "TRANSACCION";
                requestInfo[7].sValor = currentTransaction;

                requestInfo[8] = new cCampo();
                requestInfo[8].iTipo = eTipo.AN;
                requestInfo[8].sCampo = "SKU";
                requestInfo[8].sValor = SKU;

                if (Reference != string.Empty)
                {
                    requestInfo[9] = new cCampo();
                    requestInfo[9].sCampo = "REFERENCIA";
                    requestInfo[9].sValor = Encriptacion.PXEncryptFX(Reference, EncryptionKey);
                    requestInfo[9].bEncriptado = true;
                    requestInfo[9].iTipo = eTipo.AN;
                }

                if (User == string.Empty || Password == string.Empty)
                {
                    string result = "Imposible conectar al WS porque no hay credenciales";
                    return (result, 3);
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = User;
                    wservice.ClientCredentials.UserName.Password = Password;
                }

                cCampo[] response = null;

                try
                {
                    var task = Task.Run(() => wservice.InfoAsync(requestInfo));
                    
                    // Se establece el tiempo de espera para la respuesta
                    var timeout = TimeSpan.FromSeconds(45);

                    // Obtendremos: TRUE si la tarea se ejecuto dentro del tiempo establecido
                    //              FALSE si la tarea sobrepaso de ese tiempo
                    var isTaskFinished = task.Wait(timeout);

                    // Se va contabilizando los intentos para obtener respuesta del WS
                    int attempts = 1;
                    for (int i = 0; i < 3; i++)
                    {
                        //Si se obtuvo respuesta se detiene la iteracion y
                        // continuara con la logica establecida
                        if (isTaskFinished)
                        {
                            response = task.Result;
                            break;
                        }
                        else
                        {
                            //SE incremetnaran los intentos hasta obtener una respuesta
                            attempts++;

                            task = Task.Run(() => wservice.InfoAsync(requestInfo));
                        }
                    }

                    if (response.Length > 0)
                    {
                        int codeResponse = 0;
                        if (response[0].sCampo == "CODIGORESPUESTA")
                        {
                            string codeDescription;
                            codeResponse = (int)response[0].sValor;

                            if (codeResponse == (int)response[0].sValor)
                            {
                                if (response[1].sCampo == "CODIGORESPUESTADESCR")
                                {
                                    codeDescription = response[1].sValor.ToString();

                                    //Actualizar el registro de la transaccion

                                    return (codeResponse.ToString(), 3);
                                }
                                return ("Error", 3);
                            }
                        }
                    }

                    var Fields = new List<cCampo>();
                    foreach (var field in response)
                    {
                        if (field.bEncriptado == true)
                        {
                            field.sValor = Encriptacion.PXDecryptFX(field.sValor.ToString(), EncryptionKey);
                        }

                        Fields.Add(field);
                    }

                    jsonResult = JsonConvert.SerializeObject(Fields);

                    return (jsonResult, 1);
                }
                catch (System.Net.WebException wex)
                {
                    return (wex.ToString(), 3);
                }
            }
            catch (System.Net.WebException wex)
            {
                return (wex.ToString(), 3);
            }
        }
    }
}
