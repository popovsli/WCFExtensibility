using Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PocoServiceHost
{
    class Host
    {
        static void Main(string[] args)
        {
            //string baseAddress = "http://localhost:8000/";
            PocoServiceHost host = new PocoServiceHost(typeof(CalculatorService), new Uri("http://localhost:8000/PocoServiceHost/CalculatorService"));
            host.Open();

            Console.WriteLine("Press ENTER to close");
            Console.ReadLine();
            host.Close();
        }
    }
}
