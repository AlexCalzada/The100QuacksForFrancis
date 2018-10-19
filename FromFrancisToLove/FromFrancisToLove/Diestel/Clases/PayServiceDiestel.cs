using Diestel;
using FromFrancisToLove.Data;
using FromFrancisToLove.Requests.ModuleDiestel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel.Clases
{
    public class PayServiceDiestel
    {
        private string User, Password, EncriptionKey;
        private long currentTransaction;
        private string[] reverseData;
        
        public PayServiceDiestel(string[] data)
        {
            User = data[0];
            Password = data[1];
            EncriptionKey = data[2];
            currentTransaction = long.Parse(data[3]);

            //Hacemos una copia del arreglo recibido
            //para asi de esta manera mandarlo por el constructor
            //en caso de requerir hacer una reversa
            reverseData = data;
        }


        public (string response, int status) PayService(List<cCampo> campos)
        {
            string SKU = "";
            foreach (var campo in campos)
            {
                if (campo.sCampo == "SKU")
                {
                    SKU = campo.sValor.ToString();
                }
            }

            // WS Diestel
            PxUniversalSoapClient wservice = new PxUniversalSoapClient(PxUniversalSoapClient.EndpointConfiguration.PxUniversalSoap);

            // Cantidad de elementos dentro del arreglo
            int elementsAmount = campos.Count();

            // Cantidad de campos que tendra la solicitud del WS
            int index = 10 + elementsAmount;

            // Respuesta del pago procesado
            string jsonString = "";

            //Campos para la solicitud
            cCampo[] requestEjecuta = new cCampo[index];

            try
            {
                requestEjecuta[0] = new cCampo();
                requestEjecuta[0].iTipo = eTipo.NE;
                requestEjecuta[0].sCampo = "IDGRUPO";
                requestEjecuta[0].sValor = 7;

                requestEjecuta[1] = new cCampo();
                requestEjecuta[1].iTipo = eTipo.NE;
                requestEjecuta[1].sCampo = "IDCADENA";
                requestEjecuta[1].sValor = 1;

                requestEjecuta[2] = new cCampo();
                requestEjecuta[2].iTipo = eTipo.NE;
                requestEjecuta[2].sCampo = "IDTIENDA";
                requestEjecuta[2].sValor = 1;

                requestEjecuta[3] = new cCampo();
                requestEjecuta[3].iTipo = eTipo.NE;
                requestEjecuta[3].sCampo = "IDPOS";
                requestEjecuta[3].sValor = 1;

                requestEjecuta[4] = new cCampo();
                requestEjecuta[4].iTipo = eTipo.NE;
                requestEjecuta[4].sCampo = "IDCAJERO";
                requestEjecuta[4].sValor = 1;

                requestEjecuta[5] = new cCampo();
                requestEjecuta[5].iTipo = eTipo.FD;
                requestEjecuta[5].sCampo = "FECHALOCAL";
                requestEjecuta[5].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestEjecuta[6] = new cCampo();
                requestEjecuta[6].iTipo = eTipo.HR;
                requestEjecuta[6].sCampo = "HORALOCAL";
                requestEjecuta[6].sValor = DateTime.Now.ToString("HH:mm:ss");

                requestEjecuta[7] = new cCampo();
                requestEjecuta[7].iTipo = eTipo.FD;
                requestEjecuta[7].sCampo = "FECHACONTABLE";
                requestEjecuta[7].sValor = DateTime.Now.ToString("dd/MM/yyyy");

                requestEjecuta[8] = new cCampo();
                requestEjecuta[8].iTipo = eTipo.NE;
                requestEjecuta[8].sCampo = "TRANSACCION";
                requestEjecuta[8].sValor = currentTransaction;
                                
                requestEjecuta[9] = new cCampo();
                requestEjecuta[9].iTipo = eTipo.AN;
                requestEjecuta[9].sCampo = "SKU";
                requestEjecuta[9].sValor = SKU;

                //Se recorre todo el arreglo "principal" (request)
                for (int i = 0; i < requestEjecuta.Length; i++)
                {
                    //Identificamos que posicion del arreglo es nula
                    //para posteriormente inicializarla con un valor
                    if (requestEjecuta[i] == null)
                    {
                        //Se declara una variable para indicar la poscicion
                        //del arreglo recibido como parametro del metodo (info)
                        int cp = 0;

                        //Recorremos e igualamos las posiciones
                        for (int j = i; j < requestEjecuta.Length; j++)
                        {
                            //Identificamos hasta que cantidad de elementos se
                            //deberan de añadir a los espacios nulos del arreglo principal
                            if (cp <= elementsAmount)
                            {
                                //Agregamos los nuevos elementos del arreglo recibido al
                                //arreglo principal
                                requestEjecuta[j] = new cCampo();
                                requestEjecuta[j].sCampo = campos[cp].sCampo;

                                //Se filtran los elemntos que deberan ir encriptados
                                if (campos[cp].sCampo == "REFERENCIA")
                                {
                                    requestEjecuta[j].sValor = Encriptacion.PXEncryptFX(campos[cp].sValor.ToString(), EncriptionKey);
                                    requestEjecuta[j].bEncriptado = true;
                                }
                                else
                                {
                                    requestEjecuta[j].sValor = campos[cp].sValor.ToString();
                                    requestEjecuta[j].bEncriptado = false;
                                }

                                //Se incrementa la variable para pasar al siguiente elemento que se debera
                                // de añadir al arreglo
                                cp++;
                            }
                        }
                    }
                }

                //Se verifica que existan las credenciales
                if (User == string.Empty || Password == string.Empty || User == null || Password == null)
                {
                    jsonString = "Imposible conectar al WS porque no hay credenciales";
                    return (jsonString, 3);
                }
                else
                {
                    wservice.ClientCredentials.UserName.UserName = User;
                    wservice.ClientCredentials.UserName.Password = Password;
                }

                // Respuesta del WS
                cCampo[] response = null;

                try
                {

                    var task = Task.Run(() => wservice.EjecutaAsync(requestEjecuta));

                    // Se registra la solicitud de pago enviada en el TXT
                    //LogReversos.WriteTXData("+", "Solicitud de Pago enviada", currentTransaction.ToString());

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

                            //Actualizar tabla [Transaccion] con el intento

                            task = Task.Run(() => wservice.EjecutaAsync(requestEjecuta));
                        }
                    }

                    //Se recopilan los campos de la respuesta y se meten dentro de una lista
                    //en caso de requerir hacer una reversa
                    var listReverse = new List<cCampo>();
                    foreach (var campo in response)
                    {
                        listReverse.Add(campo);
                    }

                    //Verificams que obtengamos una respuesta
                    if (response.Length > 0)
                    {
                        // Validacion de respuesta del servicio en caso de [ERROR]
                        if (response[0].sCampo == "CODIGORESPUESTA")
                        {
                            string codeDescription;
                            int codeResponse = (int)response[0].sValor;

                            if (codeResponse == (int)response[0].sValor)
                            {
                                if (response[1].sCampo == "CODIGORESPUESTADESCR")
                                {
                                    codeDescription = response[1].sValor.ToString();
                                    
                                    //Actualizar tabla transacciones
                                    // el intento, CodeResponse, y el status 3

                                    return (codeResponse.ToString(), 3);
                                }
                            }
                        }
                    }
                    else
                    {
                        //
                        // + Se ACTUALIZA el registro del pago NO existoso [Transacciones]
                        //

                        if (response.Length != 0)
                        {
                            foreach (var item in response)
                            {
                                if ((int)item.sValor == 47)
                                {
                                   //Se debera devolver un "ticket"
                                   //demostrando el motivo del rechazo
                                   //de la operacion
                                   //q
                                   // Devolver un JSON a la Front-End
                                }
                                else if((int)item.sValor == 8 || (int)item.sValor == 71 || (int)item.sValor == 72)
                                {
                                    //Proceso de reversas

                                    CancelationService cancelation = new CancelationService(reverseData);
                                    var cancel = cancelation.Reverses(listReverse);
                                    return (cancel.result, 2);
                                }
                            }
                        }
                    }
                }
                catch (System.Net.WebException)
                {
                    //Proceso de reversas
                }

                //Lista con los campos que se le van a presentar 
                // a la front-end
                var listCampos = new List<object>();
                foreach (var wsCampo in response)
                {
                    if (wsCampo.sCampo == "REFERENCIA")
                    {
                        wsCampo.sValor = Encriptacion.PXDecryptFX(wsCampo.sValor.ToString(), EncriptionKey);
                    }

                    listCampos.Add(wsCampo);
                }

                try
                {
                    //Proceso de insercion a la tabla Transacciones [Existoso]
                    
                }
                catch (Exception)
                {

                    throw;
                }

                //Serializa a json
                var jsonResult = JsonConvert.SerializeObject(listCampos);
                jsonString = jsonResult;

            }
            catch (Exception)
            {

                throw;
            }
            return (jsonString, 1);
        }
    }
}
