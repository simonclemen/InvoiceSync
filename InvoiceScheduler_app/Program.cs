using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceScheduler
{
    internal class Program
    {
        static void Main(string[] args)
        {      
            var c = new InvoiceScheduler_Consumer.Consumer();
            c.Execute().Wait();

        }
    }
}
