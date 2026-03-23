using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.BookedInvoice
{
    internal class BookedInvoice
    {
        public BookedInvoice() { this.draftInvoice = new DraftInvoice(); }

      public DraftInvoice draftInvoice { get; set; }
        public string sendBy { get; internal set; }
    }
    internal class DraftInvoice
    {
        public int draftInvoiceNumber { get; internal set; }
     

    }
}
