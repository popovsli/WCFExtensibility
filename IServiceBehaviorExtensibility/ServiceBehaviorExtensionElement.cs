using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace IServiceBehaviorExtensibility
{
    class ServiceBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get
            {
                return typeof(PocoServiceBehavior);
            }
        }

        protected override object CreateBehavior()
        {
            return new PocoServiceBehavior();
        }
    }
}
