using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleTexDataTransfer_Consumer_TripleTex.Session
{
    internal class Session
    {
        public Value value { get; set; }
        public bool Success { get { return value!=null && !string.IsNullOrWhiteSpace(value.token); } }
    }

    public class ConsumerToken
    {
        public long id { get; set; }
        public string url { get; set; }
    }

    public class EmployeeToken
    {
        public long id { get; set; }
        public string url { get; set; }
    }

    public class Value
    {
        public long id { get; set; }
        public int version { get; set; }
        public string url { get; set; }
        public ConsumerToken consumerToken { get; set; }
        public EmployeeToken employeeToken { get; set; }
        public string expirationDate { get; set; }
        public string token { get; set; }
        public object encryptionKey { get; set; }
      
    }

  
}
