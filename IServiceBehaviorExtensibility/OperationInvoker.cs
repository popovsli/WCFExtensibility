using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace IServiceBehaviorExtensibility
{
    // Simple invoker, simply delegates to the underlying MethodInfo object
    public class OperationInvoker : IOperationBehavior, IOperationInvoker
    {
        MethodInfo method;

        public OperationInvoker(MethodInfo method)
        {
            this.method = method;
        }

        public object[] AllocateInputs()
        {
            return new object[method.GetParameters().Length];
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            outputs = new object[0];
            return this.method.Invoke(instance, inputs);
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Invoker = this;
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
