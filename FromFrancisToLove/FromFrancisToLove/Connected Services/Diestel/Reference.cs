//------------------------------------------------------------------------------
// <generado automáticamente>
//     Este código fue generado por una herramienta.
//     //
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </generado automáticamente>
//------------------------------------------------------------------------------

namespace Diestel
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.pagoexpress.com.mx/pxUniversal", ConfigurationName="Diestel.PxUniversalSoap")]
    public interface PxUniversalSoap
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/pxUniversal/Info", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(cCampo[]))]
        System.Threading.Tasks.Task<Diestel.cCampo[]> InfoAsync(Diestel.cCampo[] cArrayCampos);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/pxUniversal/Ejecuta", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(cCampo[]))]
        System.Threading.Tasks.Task<Diestel.cCampo[]> EjecutaAsync(Diestel.cCampo[] cArrayCampos);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/pxUniversal/Reversa", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(cCampo[]))]
        System.Threading.Tasks.Task<Diestel.cCampo[]> ReversaAsync(Diestel.cCampo[] cArrayCampos);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/pxUniversal/Test", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(cCampo[]))]
        System.Threading.Tasks.Task<Diestel.cCampo[]> TestAsync();
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.pagoexpress.com.mx/pxUniversal")]
    public partial class cCampo
    {
        
        private string sCampoField;
        
        private eTipo iTipoField;
        
        private int iLongitudField;
        
        private int iClaseField;
        
        private object sValorField;
        
        private bool bEncriptadoField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string sCampo
        {
            get
            {
                return this.sCampoField;
            }
            set
            {
                this.sCampoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public eTipo iTipo
        {
            get
            {
                return this.iTipoField;
            }
            set
            {
                this.iTipoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public int iLongitud
        {
            get
            {
                return this.iLongitudField;
            }
            set
            {
                this.iLongitudField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public int iClase
        {
            get
            {
                return this.iClaseField;
            }
            set
            {
                this.iClaseField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public object sValor
        {
            get
            {
                return this.sValorField;
            }
            set
            {
                this.sValorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public bool bEncriptado
        {
            get
            {
                return this.bEncriptadoField;
            }
            set
            {
                this.bEncriptadoField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.pagoexpress.com.mx/pxUniversal")]
    public enum eTipo
    {
        
        /// <remarks/>
        AN,
        
        /// <remarks/>
        NE,
        
        /// <remarks/>
        NM,
        
        /// <remarks/>
        FD,
        
        /// <remarks/>
        HR,
        
        /// <remarks/>
        ND,
        
        /// <remarks/>
        PW,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    public interface PxUniversalSoapChannel : Diestel.PxUniversalSoap, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    public partial class PxUniversalSoapClient : System.ServiceModel.ClientBase<Diestel.PxUniversalSoap>, Diestel.PxUniversalSoap
    {
        
    /// <summary>
    /// Implemente este método parcial para configurar el punto de conexión de servicio.
    /// </summary>
    /// <param name="serviceEndpoint">El punto de conexión para configurar</param>
    /// <param name="clientCredentials">Credenciales de cliente</param>
    static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public PxUniversalSoapClient(EndpointConfiguration endpointConfiguration) : 
                base(PxUniversalSoapClient.GetBindingForEndpoint(endpointConfiguration), PxUniversalSoapClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public PxUniversalSoapClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(PxUniversalSoapClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public PxUniversalSoapClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(PxUniversalSoapClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public PxUniversalSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<Diestel.cCampo[]> InfoAsync(Diestel.cCampo[] cArrayCampos)
        {
            return base.Channel.InfoAsync(cArrayCampos);
        }
        
        public System.Threading.Tasks.Task<Diestel.cCampo[]> EjecutaAsync(Diestel.cCampo[] cArrayCampos)
        {
            return base.Channel.EjecutaAsync(cArrayCampos);
        }
        
        public System.Threading.Tasks.Task<Diestel.cCampo[]> ReversaAsync(Diestel.cCampo[] cArrayCampos)
        {
            return base.Channel.ReversaAsync(cArrayCampos);
        }
        
        public System.Threading.Tasks.Task<Diestel.cCampo[]> TestAsync()
        {
            return base.Channel.TestAsync();
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.PxUniversalSoap))
            {
                //System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly);
                result.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                result.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                return result;
            }
            if ((endpointConfiguration == EndpointConfiguration.PxUniversalSoap12))
            {
                System.ServiceModel.Channels.CustomBinding result = new System.ServiceModel.Channels.CustomBinding();
                System.ServiceModel.Channels.TextMessageEncodingBindingElement textBindingElement = new System.ServiceModel.Channels.TextMessageEncodingBindingElement();
                textBindingElement.MessageVersion = System.ServiceModel.Channels.MessageVersion.CreateVersion(System.ServiceModel.EnvelopeVersion.Soap12, System.ServiceModel.Channels.AddressingVersion.None);
                result.Elements.Add(textBindingElement);
                System.ServiceModel.Channels.HttpsTransportBindingElement httpsBindingElement = new System.ServiceModel.Channels.HttpsTransportBindingElement();
                httpsBindingElement.AllowCookies = true;
                httpsBindingElement.MaxBufferSize = int.MaxValue;
                httpsBindingElement.MaxReceivedMessageSize = int.MaxValue;
                result.Elements.Add(httpsBindingElement);
                return result;
            }
            throw new System.InvalidOperationException(string.Format("No se pudo encontrar un punto de conexión con el nombre \"{0}\".", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.PxUniversalSoap))
            {
                return new System.ServiceModel.EndpointAddress("https://dev.integracionesqapx.com.mx/wsUniversal/pxUniversal.asmx");
            }
            if ((endpointConfiguration == EndpointConfiguration.PxUniversalSoap12))
            {
                return new System.ServiceModel.EndpointAddress("https://dev.integracionesqapx.com.mx/wsUniversal/pxUniversal.asmx");
            }
            throw new System.InvalidOperationException(string.Format("No se pudo encontrar un punto de conexión con el nombre \"{0}\".", endpointConfiguration));
        }
        
        public enum EndpointConfiguration
        {
            
            PxUniversalSoap,
            
            PxUniversalSoap12,
        }
    }
}
