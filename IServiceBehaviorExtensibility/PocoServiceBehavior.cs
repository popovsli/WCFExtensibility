using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace IServiceBehaviorExtensibility
{
    public class PocoServiceBehavior : IServiceBehavior
    {
        const string DefaultNamespace = "http://tempuri.org/";
        static readonly Func<Binding> createBinding = () => new BasicHttpBinding();

        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            Type serviceType = serviceDescription.ServiceType;
            ContractDescription contractDescription = CreateContractDescription(serviceType);
            string endpointAddress = serviceHostBase.BaseAddresses[0].AbsoluteUri;

            ServiceEndpoint endpoint = new ServiceEndpoint(contractDescription, createBinding(), new EndpointAddress(new Uri(endpointAddress)));
            serviceDescription.Endpoints.Add(endpoint);

            ChannelDispatcher dispatcher = CreateChannelDispatcher(endpoint, serviceType);
            serviceHostBase.ChannelDispatchers.Add(dispatcher);

            //AssociateEndpointToDispatcher(endpoint, dispatcher);
        }

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            Type serviceType = serviceDescription.ServiceType;
            ConstructorInfo ctor = serviceType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            if (ctor == null)
            {
                throw new InvalidOperationException("Service must have a parameterless, public constructor.");
            }

            MethodInfo[] methods = serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.DeclaringType == serviceType).ToArray();
            if (methods.Length == 0)
            {
                throw new InvalidOperationException("Service does not have any public methods.");
            }

            foreach (MethodInfo method in methods)
            {
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    if (parameter.ParameterType.IsByRef)
                    {
                        throw new InvalidOperationException("This behavior does not support public methods with out/ref parameters.");
                    }
                }
            }
        }

        /// <summary>
        /// CreateContractDescription walks through the public methods of the service, and add an operation description to the contract. 
        /// </summary>
        /// <param name="serviceType" ></param>
        /// <returns>ContractDescription</returns>
        private ContractDescription CreateContractDescription(Type serviceType)
        {
            ContractDescription result = new ContractDescription(serviceType.Name, DefaultNamespace);
            result.ContractType = serviceType;
            foreach (MethodInfo method in serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.DeclaringType == serviceType)
                {
                    result.Operations.Add(CreateOperationDescription(result, serviceType, method));
                }
            }

            return result;
        }
        
        private OperationDescription CreateOperationDescription(ContractDescription contract, Type serviceType, MethodInfo method)
        {
            OperationDescription result = new OperationDescription(method.Name, contract);
            result.SyncMethod = method;

            MessageDescription inputMessage = new MessageDescription(DefaultNamespace + serviceType.Name + "/" + method.Name, MessageDirection.Input);
            inputMessage.Body.WrapperNamespace = DefaultNamespace;
            inputMessage.Body.WrapperName = method.Name;
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                MessagePartDescription part = new MessagePartDescription(parameter.Name, DefaultNamespace);
                part.Type = parameter.ParameterType;
                part.Index = i;
                inputMessage.Body.Parts.Add(part);
            }

            result.Messages.Add(inputMessage);

            MessageDescription outputMessage = new MessageDescription(DefaultNamespace + serviceType.Name + "/" + method.Name + "Response", MessageDirection.Output);
            outputMessage.Body.WrapperName = method.Name + "Response";
            outputMessage.Body.WrapperNamespace = DefaultNamespace;
            outputMessage.Body.ReturnValue = new MessagePartDescription(method.Name + "Result", DefaultNamespace);
            outputMessage.Body.ReturnValue.Type = method.ReturnType;
            result.Messages.Add(outputMessage);

            result.Behaviors.Add(new OperationInvoker(method));
            result.Behaviors.Add(new OperationBehaviorAttribute());
            result.Behaviors.Add(new DataContractSerializerOperationBehavior(result));

            return result;
        }

        private ChannelDispatcher CreateChannelDispatcher(ServiceEndpoint endpoint, Type serviceType)
        {
            EndpointAddress address = endpoint.Address;
            BindingParameterCollection bindingParameters = new BindingParameterCollection();
            IChannelListener channelListener = endpoint.Binding.BuildChannelListener<IReplyChannel>(address.Uri, bindingParameters);
            ChannelDispatcher channelDispatcher = new ChannelDispatcher(channelListener, endpoint.Binding.Namespace + ":" + endpoint.Binding.Name, endpoint.Binding);
            channelDispatcher.MessageVersion = endpoint.Binding.MessageVersion;

            EndpointDispatcher endpointDispatcher = new EndpointDispatcher(address, endpoint.Contract.Name, endpoint.Contract.Namespace, false);
            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                string replyAction = operation.Messages.Count > 1 ? operation.Messages[1].Action : "";
                DispatchOperation operationDispatcher = new DispatchOperation(endpointDispatcher.DispatchRuntime, operation.Name, operation.Messages[0].Action, replyAction);
                foreach (IOperationBehavior operationBehavior in operation.Behaviors)
                {
                    operationBehavior.ApplyDispatchBehavior(operation, operationDispatcher);
                }

                endpointDispatcher.DispatchRuntime.Operations.Add(operationDispatcher);
            }

            channelDispatcher.Endpoints.Add(endpointDispatcher);
            return channelDispatcher;
        }

    }
}
