using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation
    {
    abstract class ResidentAddressPopulation : AddressPopulation
        {
        private static Dictionary<string, Resident> residentDictionary = new Dictionary<string, Resident>();

        public static Dictionary<string, Resident> ResidentDictionary { get => residentDictionary; set => residentDictionary = value; }

        //       abstract public void buildAndSaveTrashRecycleEntitiesFromRequest(dynamic request, IEnumerator<KGISAddress> foundKgisResidentAddressEnumerator, int numberOfUnits);

        private static Dictionary<string, int> residentIdentiferDictionary = new Dictionary<string, int>();
        public static Dictionary<string, int> ResidentIdentiferDictionary { get => residentIdentiferDictionary; set => residentIdentiferDictionary = value; }

        static public Regex reducePhoneNumber = new Regex("[^\\d]");

        public Resident buildResidentFromRequest(dynamic request)
            {
            Resident resident = new Resident();
            resident.Email = request.Email;
            resident.FirstName = request.FirstName;
            resident.LastName = request.LastName;

            if (request.GetType().GetProperty("PhoneNumber") != null)
                {
                if (!String.IsNullOrWhiteSpace(resident.Phone))
                    {
                    string phoneNumber = reducePhoneNumber.Replace(request.PhoneNumber, string.Empty).Trim();
                    resident.Phone = phoneNumber;
                    }
                }

            if ((request.GetType().GetProperty("SendEmailNewsletter") != null))
                {
                if (String.IsNullOrEmpty(request.SendEmailNewsletter) || request.SendEmailNewsletter.Equals("N"))
                    {
                    resident.SendEmailNewsletter = false;
                    }
                else
                    {
                    resident.SendEmailNewsletter = true;
                    }
                }
            else
                {
                resident.SendEmailNewsletter = false;
                }
            resident.CreateDate = request.CreationDate;
            resident.CreateUser = request.CreatedBy;
            resident.UpdateDate = request.LastUpdatedDate;
            resident.UpdateUser = request.LastUpdatedBy;
            return resident;
            }



        
        }
    }
