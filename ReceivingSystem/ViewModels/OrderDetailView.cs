using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    public class OrderDetailView
    {
        public int PartId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int OrderQty { get; set; }
        public int PurchaseOrderId { get; set; }


        //current from DB
        public int ReceivedToDate { get; set; }
        public int ReturnedToDate { get; set; }
        public string LastReturnReason { get; set; } = string.Empty;

        //user edit
        public int Received { get; set; } = 0;
        public int Returned { get; set; } = 0;
        public string Reason { get; set; } = string.Empty;

        public bool HasError { get; set; }
        public bool HasEditedReceived { get; set; } = false;

        // Computed properties
        // Outstanding quantity based on received to date
        public int OutstandingBase => Math.Max(0, OrderQty - ReceivedToDate);

        //shows 0 if received is negative, otherwise calculates outstanding based on received
        public int Outstanding => (Received < 0) ? 0 : Math.Max(0, OutstandingBase - Received);

        //added for easier access to PurchaseOrderDetailID
        public int PurchaseOrderDetailID { get; set; }
    }
}
