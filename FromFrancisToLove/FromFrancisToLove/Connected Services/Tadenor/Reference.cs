//------------------------------------------------------------------------------
// <generado automáticamente>
//     Este código fue generado por una herramienta.
//     //
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </generado automáticamente>
//------------------------------------------------------------------------------

namespace Tadenor
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.pagoexpress.com.mx/ServicePX", ConfigurationName="Tadenor.ServicePXSoap")]
    public interface ServicePXSoap
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/StatusPTVAnd", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> StatusPTVAndAsync(string sXML);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/ConfInicialAnd", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> ConfInicialAndAsync(string sXML);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/SaldoDisponible", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<double> SaldoDisponibleAsync(long lGrupo, long lCadena, long lTienda);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/getReloadClass", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> getReloadClassAsync(string sXML);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/getReloadData", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> getReloadDataAsync(string sXML);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/getReverseClass", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> getReverseClassAsync(string sXML);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/getQueryClass", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> getQueryClassAsync(string sXML);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.pagoexpress.com.mx/ServicePX/getQueryDatClass", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> getQueryDatClassAsync(string sXML);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    public interface ServicePXSoapChannel : Tadenor.ServicePXSoap, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.0")]
    public partial class ServicePXSoapClient : System.ServiceModel.ClientBase<Tadenor.ServicePXSoap>, Tadenor.ServicePXSoap
    {
        
    /// <summary>
    /// Implemente este método parcial para configurar el punto de conexión de servicio.
    /// </summary>
    /// <param name="serviceEndpoint">El punto de conexión para configurar</param>
    /// <param name="clientCredentials">Credenciales de cliente</param>
    static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public ServicePXSoapClient(EndpointConfiguration endpointConfiguration) : 
                base(ServicePXSoapClient.GetBindingForEndpoint(endpointConfiguration), ServicePXSoapClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public ServicePXSoapClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(ServicePXSoapClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public ServicePXSoapClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(ServicePXSoapClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public ServicePXSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<string> StatusPTVAndAsync(string sXML)
        {
            return base.Channel.StatusPTVAndAsync(sXML);
        }
        
        public System.Threading.Tasks.Task<string> ConfInicialAndAsync(string sXML)
        {
            return base.Channel.ConfInicialAndAsync(sXML);
        }
        
        public System.Threading.Tasks.Task<double> SaldoDisponibleAsync(long lGrupo, long lCadena, long lTienda)
        {
            return base.Channel.SaldoDisponibleAsync(lGrupo, lCadena, lTienda);
        }
        
        public System.Threading.Tasks.Task<string> getReloadClassAsync(string sXML)
        {
            return base.Channel.getReloadClassAsync(sXML);
        }
        
        public System.Threading.Tasks.Task<string> getReloadDataAsync(string sXML)
        {
            return base.Channel.getReloadDataAsync(sXML);
        }
        
        public System.Threading.Tasks.Task<string> getReverseClassAsync(string sXML)
        {
            return base.Channel.getReverseClassAsync(sXML);
        }
        
        public System.Threading.Tasks.Task<string> getQueryClassAsync(string sXML)
        {
            return base.Channel.getQueryClassAsync(sXML);
        }
        
        public System.Threading.Tasks.Task<string> getQueryDatClassAsync(string sXML)
        {
            return base.Channel.getQueryDatClassAsync(sXML);
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
            if ((endpointConfiguration == EndpointConfiguration.ServicePXSoap))
            {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                return result;
            }
            if ((endpointConfiguration == EndpointConfiguration.ServicePXSoap12))
            {
                System.ServiceModel.Channels.CustomBinding result = new System.ServiceModel.Channels.CustomBinding();
                System.ServiceModel.Channels.TextMessageEncodingBindingElement textBindingElement = new System.ServiceModel.Channels.TextMessageEncodingBindingElement();
                textBindingElement.MessageVersion = System.ServiceModel.Channels.MessageVersion.CreateVersion(System.ServiceModel.EnvelopeVersion.Soap12, System.ServiceModel.Channels.AddressingVersion.None);
                result.Elements.Add(textBindingElement);
                System.ServiceModel.Channels.HttpTransportBindingElement httpBindingElement = new System.ServiceModel.Channels.HttpTransportBindingElement();
                httpBindingElement.AllowCookies = true;
                httpBindingElement.MaxBufferSize = int.MaxValue;
                httpBindingElement.MaxReceivedMessageSize = int.MaxValue;
                result.Elements.Add(httpBindingElement);
                return result;
            }
            throw new System.InvalidOperationException(string.Format("No se pudo encontrar un punto de conexión con el nombre \"{0}\".", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.ServicePXSoap))
            {
                return new System.ServiceModel.EndpointAddress("http://tndesarollo.com/WS_pruebasTN/ServicePX.asmx");
            }
            if ((endpointConfiguration == EndpointConfiguration.ServicePXSoap12))
            {
                return new System.ServiceModel.EndpointAddress("http://tndesarollo.com/WS_pruebasTN/ServicePX.asmx");
            }
            throw new System.InvalidOperationException(string.Format("No se pudo encontrar un punto de conexión con el nombre \"{0}\".", endpointConfiguration));
        }
        
        public enum EndpointConfiguration
        {
            
            ServicePXSoap,
            
            ServicePXSoap12,
        }
    }
}
