using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    public class SNSpreadsheetRow
        {
        Address address = new Address();
        int rowNumber = -1;
        string currentTrashCartDeliveryDate = "";
        string currentTrashCartSN = "";
        string firstTrashCartDeliveryDate = "";
        string firstTrashCartSN = "";
        string secondTrashCartDeliveryDate = "";
        string secondTrashCartSN = "";
        string thirdTrashCartDeliveryDate = "";
        string thirdTrashCartSN = "";
        string fourthTrashCartDeliveryDate = "";
        string fourthTrashCartSN = "";
        string currentRecycleCartDeliveryDate = "";
        string currentRecycleCartSN = "";
        string firstRecycleCartDeliveryDate = "";
        string firstRecycleCartSN = "";
        string secondRecycleCartDeliveryDate = "";
        string secondRecycleCartSN = "";
        string thirdRecycleCartDeliveryDate = "";
        string thirdRecycleCartSN = "";
        string multiFamilyUnit = "";
        string commercialAccount = "";
        string smallTrashCart = "";
        string specialty = "";
        string addressPreviouslyDemolished = "";
        string disapproved = "";
        string noCartHere = "";
        string duplicateAddress = "";
        string notes = "";

        public Address Address { get => address; set => address = value; }
        public string CurrentTrashCartDeliveryDate { get => currentTrashCartDeliveryDate; set => currentTrashCartDeliveryDate = value; }
        public string CurrentTrashCartSN { get => currentTrashCartSN; set => currentTrashCartSN = value; }
        public string FirstTrashCartDeliveryDate { get => firstTrashCartDeliveryDate; set => firstTrashCartDeliveryDate = value; }
        public string FirstTrashCartSN { get => firstTrashCartSN; set => firstTrashCartSN = value; }
        public string SecondTrashCartDeliveryDate { get => secondTrashCartDeliveryDate; set => secondTrashCartDeliveryDate = value; }
        public string SecondTrashCartSN { get => secondTrashCartSN; set => secondTrashCartSN = value; }
        public string ThirdTrashCartDeliveryDate { get => thirdTrashCartDeliveryDate; set => thirdTrashCartDeliveryDate = value; }
        public string ThirdTrashCartSN { get => thirdTrashCartSN; set => thirdTrashCartSN = value; }
        public string CurrentRecycleCartDeliveryDate { get => currentRecycleCartDeliveryDate; set => currentRecycleCartDeliveryDate = value; }
        public string CurrentRecycleCartSN { get => currentRecycleCartSN; set => currentRecycleCartSN = value; }
        public string FirstRecycleCartDeliveryDate { get => firstRecycleCartDeliveryDate; set => firstRecycleCartDeliveryDate = value; }
        public string FirstRecycleCartSN { get => firstRecycleCartSN; set => firstRecycleCartSN = value; }
        public string SecondRecycleCartDeliveryDate { get => secondRecycleCartDeliveryDate; set => secondRecycleCartDeliveryDate = value; }
        public string SecondRecycleCartSN { get => secondRecycleCartSN; set => secondRecycleCartSN = value; }
        public string MultiFamilyUnit { get => multiFamilyUnit; set => multiFamilyUnit = value; }
        public string CommercialAccount { get => commercialAccount; set => commercialAccount = value; }
        public string SmallTrashCart { get => smallTrashCart; set => smallTrashCart = value; }
        public string Specialty { get => specialty; set => specialty = value; }
        public string AddressPreviouslyDemolished { get => addressPreviouslyDemolished; set => addressPreviouslyDemolished = value; }
        public string Disapproved { get => disapproved; set => disapproved = value; }
        public string NoCartHere { get => noCartHere; set => noCartHere = value; }
        public string DuplicateAddress { get => duplicateAddress; set => duplicateAddress = value; }
        public string Notes { get => notes; set => notes = value; }
        public int RowNumber { get => rowNumber; set => rowNumber = value; }
        public string FourthTrashCartDeliveryDate { get => fourthTrashCartDeliveryDate; set => fourthTrashCartDeliveryDate = value; }
        public string FourthTrashCartSN { get => fourthTrashCartSN; set => fourthTrashCartSN = value; }
        public string ThirdRecycleCartDeliveryDate { get => thirdRecycleCartDeliveryDate; set => thirdRecycleCartDeliveryDate = value; }
        public string ThirdRecycleCartSN { get => thirdRecycleCartSN; set => thirdRecycleCartSN = value; }
        }
    }
