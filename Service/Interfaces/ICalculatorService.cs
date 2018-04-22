using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
   // [ServiceContract]
    public interface ICalculatorService
    {
       // [OperationContract]
        int Divide(int a, int b);
    }
}
