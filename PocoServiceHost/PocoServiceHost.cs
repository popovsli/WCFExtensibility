using IServiceBehaviorExtensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace PocoServiceHost
{
    public class PocoServiceHost : ServiceHost
    {
        public PocoServiceHost(Type serviceType, Uri baseAddress)
            : base(serviceType, baseAddress)
        {

        }

        public PocoServiceHost(Type serviceType)
            : base(serviceType)
        {

        }


        protected override void InitializeRuntime()
        {
            this.Description.Behaviors.Insert(0, new PocoServiceBehavior());
            this.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
            base.InitializeRuntime();
        }

    }
}
