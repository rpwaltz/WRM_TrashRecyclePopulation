using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRM_TrashRecyclePopulation
    {
    class CommercialAccountRow
        {
        string billingNote ="";
        string dateAdded ="";
        string dateTerminated ="";
        string outstandingBalanceOwned = "";
        string status ="";
        string isRecycler ="";
        string serviceDays ="";
        string customerNumber ="";
        string customerName ="";
        string serviceAddress ="";
        string serviceZipCode ="";
        string billingStreetNumber ="";
        string billingCity ="";
        string billingState ="";
        string billingZipCode ="";
        string personOfContact ="";
        string contactPhoneNumber ="";
        string contactEmailAddress ="";
        string billingRate = "";
        string numberOfTrashCarts = "";
        string numberOfRecyclingCarts = "";
        string accountNotes ="";
        bool isTermminated = false;
        bool hasDowntownCrewPickup = false;
        public string BillingNote { get => billingNote; set => billingNote = value; }
        public string DateAdded { get => dateAdded; set => dateAdded = value; }
        public string DateTerminated { get => dateTerminated; set => dateTerminated = value; }
        public string Status { get => status; set => status = value; }
        public string IsRecycler { get => isRecycler; set => isRecycler = value; }
        public string ServiceDays { get => serviceDays; set => serviceDays = value; }
        public string CustomerNumber { get => customerNumber; set => customerNumber = value; }
        public string CustomerName { get => customerName; set => customerName = value; }
        public string ServiceAddress { get => serviceAddress; set => serviceAddress = value; }
        public string ServiceZipCode { get => serviceZipCode; set => serviceZipCode = value; }
        public string BillingStreetNumber { get => billingStreetNumber; set => billingStreetNumber = value; }
        public string BillingCity { get => billingCity; set => billingCity = value; }
        public string BillingState { get => billingState; set => billingState = value; }
        public string BillingZipCode { get => billingZipCode; set => billingZipCode = value; }
        public string PersonOfContact { get => personOfContact; set => personOfContact = value; }
        public string ContactPhoneNumber { get => contactPhoneNumber; set => contactPhoneNumber = value; }
        public string ContactEmailAddress { get => contactEmailAddress; set => contactEmailAddress = value; }
        public string BillingRate { get => billingRate; set => billingRate = value; }
        public string NumberOfTrashCarts { get => numberOfTrashCarts; set => numberOfTrashCarts = value; }
        public string NumberOfRecyclingCarts { get => numberOfRecyclingCarts; set => numberOfRecyclingCarts = value; }
        public string AccountNotes { get => accountNotes; set => accountNotes = value; }
        public string OutstandingBalanceOwned { get => outstandingBalanceOwned; set => outstandingBalanceOwned = value; }
        public bool IsTermminated { get => isTermminated; set => isTermminated = value; }
        public bool HasDowntownCrewPickup { get => hasDowntownCrewPickup; set => hasDowntownCrewPickup = value; }
        }
    }
