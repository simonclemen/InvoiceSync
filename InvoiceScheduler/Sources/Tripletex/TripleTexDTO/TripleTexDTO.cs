
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TripleTexDataTransfer_Consumer_TripleTex.Customer;
using TripleTexDataTransfer_Consumer_TripleTex.Invoice;
using TripleTexDataTransfer_Consumer_TripleTex.Order;
using TripleTexDataTransfer_Consumer_TripleTex.Product;


namespace TripleTexDataTransfer_Consumer_TripleTex
{
    //TripleTexDataTransfer_Consumer_Economic
    public class CombinedDataSet
    {
        public CombinedDataSet()
        {
        
        }

        public CombinedDataSet(ProductResponse products, CustomerResponse customers, InvoiceResponse invoices)
        {
            Products = products;
            Customers = customers;
            Invoices = invoices;
  
        }

        public ProductResponse Products { get; set; }
        public CustomerResponse Customers { get; set; }
        public InvoiceResponse Invoices { get; set; }
        public OrderResponse Orders { get; internal set; }
    }


   
    public abstract class GenericResponse<T> where T : GenericDataResponse
    {
        public int fullResultSize { get; set; }
        public int from { get; set; }
        public int count { get; set; }
        public string versionDigest { get; set; }
        public List<T> values { get; set; }

        public bool Success { get { return values != null; } }

    }
    public abstract class GenericDataResponse
    {
        public int id { get; set; }
      //  public string Hash { get; set; }
       // public string Json { get; internal set; }

        public override string ToString()
        {
            var s = "";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(this);
                s = s + (property.Name ?? string.Empty) + ":" + (propertyValue ?? string.Empty) + "_";
            }

            return s;
        }
        public string C_SQL()
        {
            var s = "";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(this);
                s = s + "parms.Add(name: \"@" + property.Name + "\", value: data." + property.Name + ", direction: System.Data.ParameterDirection.Input);" + System.Environment.NewLine;//(property.Name ?? string.Empty) + ":" + (propertyValue ?? string.Empty) + "_";
            }

            return s;
        }
        public string ToAddColumns()
        {
            var s = "";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                s = s + "ALTER TABLE dbo.[" + type.ToString() + "] ADD [" + property.Name + "] nvarchar(max);";
                //object propertyValue = property.GetValue(this);
                //s = s + (property.Name ?? string.Empty) + ":" + (propertyValue ?? string.Empty) + "_";
            }

            return s;
        }
    }


}
