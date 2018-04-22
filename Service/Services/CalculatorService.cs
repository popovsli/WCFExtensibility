using IServiceBehaviorExtensibility;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class CalculatorService
    {
        public int Divide(int a, int b)
        {
            return a / b;
        }
    }
}
