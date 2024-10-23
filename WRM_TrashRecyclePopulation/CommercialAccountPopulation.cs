using OfficeOpenXml;
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
    class CommercialAccountPopulation : AddressPopulation
        {
        // associated an addressID with a resident record 
        private static Dictionary<int, CommercialAccount> commercialAccountDictionary = new Dictionary<int, CommercialAccount>();

        public static Dictionary<int, CommercialAccount> CommercialAccountDictionary { get => commercialAccountDictionary; set => commercialAccountDictionary = value; }

        public static Regex addressMatchRegex = new Regex(@"(\d+)?\s*([^,]+),?\s*(.*)");
        public static Regex serviceDaysMatchRegex = new Regex(@"([A-Z]+)(?:\/\s*([A-Z]+)\s*([AB])\s*WEEK)?");

        public static List<CommercialAccountRow> CommercialAccountRowList { get => commercialAccountRowList; set => commercialAccountRowList = value; }

        private static List<CommercialAccountRow> commercialAccountRowList = new List<CommercialAccountRow>();

        CommercialAccountImporter commercialAccountImporter = new CommercialAccountImporter();

        public CommercialAccountPopulation()
            {

            addActiveCommercialAccountFromWorksheetIntoDictionary(commercialAccountImporter.ActiveCommercialAccountWorksheet);
            addTerminatedCommercialAccountFromWorksheetIntoDictionary(commercialAccountImporter.TerminatedCommercialAccountWorksheet);
            addDowntownCrewCommercialAccountFromWorksheetIntoDictionary(commercialAccountImporter.DowntownPickupCrewCommercialAccountWorksheet);
            }

        private void addActiveCommercialAccountFromWorksheetIntoDictionary(ExcelWorksheet worksheet)
            {
            int rowCount = worksheet.Dimension.End.Row;
            for (int row = 2; row <= rowCount; row++)
                {
                try
                    {
                    CommercialAccountRow commercialAccountRow = commercialAccountImporter.populateActiveCommercialAccountDictionary(worksheet, row);
                    commercialAccountRow.IsTermminated = false;
                    commercialAccountRow.HasDowntownCrewPickup = false;
                    CommercialAccountRowList.Add(commercialAccountRow);
                    }
                catch (WRMNullValueException ex)
                    {
                    // serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 8].Value = "False";
                    WRMLogger.LogBuilder.AppendLine("TRASH ADDRESS Null Value: At row " + row + " " + ex.Message);
                    continue;
                    }

                }

            }

        private void addTerminatedCommercialAccountFromWorksheetIntoDictionary(ExcelWorksheet worksheet)
            {
            int rowCount = worksheet.Dimension.End.Row;
            for (int row = 2; row <= rowCount; row++)
                {
                try
                    {
                    CommercialAccountRow commercialAccountRow = commercialAccountImporter.populateActiveCommercialAccountDictionary(worksheet, row);
                    CommercialAccountRowList.Add(commercialAccountRow);
                    commercialAccountRow.IsTermminated = true;
                    commercialAccountRow.HasDowntownCrewPickup = false;
                    }
                catch (WRMNullValueException ex)
                    {
                    // serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 8].Value = "False";
                    // WRMLogger.LogBuilder.AppendLine("TRASH ADDRESS Null Value: At row " + row + " " + ex.Message);
                    continue;
                    }

                }

            }

        private void addDowntownCrewCommercialAccountFromWorksheetIntoDictionary(ExcelWorksheet worksheet)
            {
            int rowCount = worksheet.Dimension.End.Row;
            for (int row = 2; row <= rowCount; row++)
                {
                try
                    {
                    CommercialAccountRow commercialAccountRow = commercialAccountImporter.populateActiveCommercialAccountDictionary(worksheet, row);
                    CommercialAccountRowList.Add(commercialAccountRow);
                    commercialAccountRow.IsTermminated = false;
                    commercialAccountRow.HasDowntownCrewPickup = true;
                    }
                catch (WRMNullValueException ex)
                    {
                    // serviceTrashDayImporter.TrashServiceDayWorksheet.Cells[row, 8].Value = "False";
                    // WRMLogger.LogBuilder.AppendLine("TRASH ADDRESS Null Value: At row " + row + " " + ex.Message);
                    continue;
                    }

                }

            }

        public void populateCommercialAccounts()
            {

            foreach (CommercialAccountRow commercialAccountRow in CommercialAccountRowList)
                {
                CommercialAccount commercialAccount = new CommercialAccount();

                if (commercialAccountRow.HasDowntownCrewPickup)
                    {
                    commercialAccount.HasDowntownCrewPickup = true;
                    }
                else
                    {
                    commercialAccount.HasDowntownCrewPickup = false;
                    }
                switch (commercialAccountRow.Status.ToUpper())
                    {
                    case "W":
                            {
                            commercialAccount.CommercialAccountStatus = "WITHDRAWN";
                            break;
                            }
                    case "A":
                            {
                            commercialAccount.CommercialAccountStatus = "ACTIVE";
                            break;
                            }
                    case "NA":
                            {
                            commercialAccount.CommercialAccountStatus = "ACTIVE";
                            commercialAccount.BillingNote = "NEW ACCOUNT (NEED TO BILL FOR 1ST TIME)";
                            break;
                            }
                    case "S":
                            {
                            commercialAccount.CommercialAccountStatus = "SUSPENDED";
                            break;
                            }
                    case "T":
                            {
                            commercialAccount.CommercialAccountStatus = "BANNED";
                            break;
                            }
                    case "R":
                            {
                            commercialAccount.CommercialAccountStatus = "REQUESTED";
                            break;
                            }
                    default:
                            {
                            continue;
                            }
                    }
                if (commercialAccountRow.IsTermminated)
                    {
                    commercialAccount.CommercialAccountStatus = "BANNED";
                    }
                if (!String.IsNullOrEmpty(commercialAccountRow.BillingRate))
                    {
                    commercialAccount.BillingNote = "Billing Rate $" + commercialAccountRow.BillingRate;
                    }
                if (String.IsNullOrEmpty(commercialAccountRow.BillingNote))
                    {
                    if (!String.IsNullOrEmpty(commercialAccount.BillingNote))
                        {
                        commercialAccount.BillingNote = commercialAccount.BillingNote + "\n" + commercialAccountRow.BillingNote;
                        }
                    else
                        {
                        commercialAccount.BillingNote = commercialAccountRow.BillingNote;
                        }
                    }
                if (!String.IsNullOrEmpty(commercialAccountRow.AccountNotes))
                    {
                    if (!String.IsNullOrEmpty(commercialAccount.BillingNote))
                        {
                        commercialAccount.BillingNote = commercialAccount.BillingNote + "\n" + commercialAccountRow.AccountNotes;
                        }
                    else
                        {
                        commercialAccount.BillingNote = commercialAccountRow.AccountNotes;
                        }
                    }
                commercialAccount.CommercialAccountName = commercialAccountRow.CustomerName;
                commercialAccount.CommercialAccountNumber = commercialAccountRow.CustomerNumber;


                string serviceStreetName = null;
                int serviceStreetNumber = 0;
                string serviceZipCode = null;
                string serviceUnitNumber = null;
                string addressAlternateSchedule = null;

                string serviceDaysWeeks = commercialAccountRow.ServiceDays;
                string trashServiceDay = null;
                string recyclingServiceDay = null;
                string recyclingServiceWeek = null;
                MatchCollection matchCollection = serviceDaysMatchRegex.Matches(serviceDaysWeeks);

                foreach (Match m in matchCollection)
                    {
                    if (m.Groups[1].Success)
                        {
                        trashServiceDay = m.Groups[1].Value;
                        }
                    if (commercialAccountRow.IsRecycler.Equals("Y"))
                        {
                        if (m.Groups[2].Success)
                            {
                            recyclingServiceDay = m.Groups[2].Value;
                            }
                        if (m.Groups[3].Success)
                            {
                            recyclingServiceWeek = m.Groups[3].Value;
                            }
                        }
                    }
                if (String.IsNullOrEmpty(trashServiceDay))
                    {
                    trashServiceDay = null;
                    }
                if (String.IsNullOrEmpty(recyclingServiceDay))
                    {
                    recyclingServiceDay = null;
                    }
                if (String.IsNullOrEmpty(recyclingServiceWeek))
                    {
                    recyclingServiceWeek = null;
                    }
                //WRMLogger.LogBuilder.AppendLine("Spreadsheet Recycling Day Week =" + recyclingServiceDay + "/" + recyclingServiceWeek + "- Trash Day =" + trashServiceDay + "-");

                string fullBillingStreetAddress = commercialAccountRow.BillingStreetNumber;
                int? billingStreetNumber = null;
                string billingStreetName = null;
                string billingUnitNumber = null;
                foreach (Match m in addressMatchRegex.Matches(fullBillingStreetAddress))
                    {
                    if (m.Groups[1].Success)
                        {
                        billingStreetNumber = int.Parse(m.Groups[1].Value);
                        }
                    if (m.Groups[2].Success)
                        {
                        billingStreetName = m.Groups[2].Value;
                        }
                    if (m.Groups[3].Success)
                        {
                        billingUnitNumber = m.Groups[3].Value;
                        }

                    }
                commercialAccount.CommercialCity = commercialAccountRow.BillingCity;
                commercialAccount.CommercialState = commercialAccountRow.BillingState;
                commercialAccount.CommercialStreetNumber = billingStreetNumber;
                commercialAccount.CommercialStreetName = billingStreetName;
                commercialAccount.CommercialUnitNumber = billingUnitNumber;
                commercialAccount.CommercialZipCode = commercialAccountRow.BillingZipCode;
                // WRMLogger.LogBuilder.AppendLine("Billing Address " + billingStreetNumber + " " + billingStreetName + " " + billingUnitNumber);

                if (!String.IsNullOrEmpty(commercialAccountRow.ServiceAddress))
                    {
                    string serviceAddress = commercialAccountRow.ServiceAddress;
                    foreach (Match m in addressMatchRegex.Matches(serviceAddress))
                        {
                        if (m.Groups[1].Success)
                            {
                            serviceStreetNumber = int.Parse(m.Groups[1].Value);
                            }
                        if (m.Groups[2].Success)
                            {
                            serviceStreetName = m.Groups[2].Value;
                            }
                        if (m.Groups[3].Success)
                            {
                            serviceUnitNumber = m.Groups[3].Value;
                            }
                        serviceZipCode = commercialAccountRow.ServiceZipCode;
                        }
                    // WRMLogger.LogBuilder.AppendLine("Service Address " + serviceStreetNumber + " " + serviceStreetName + " " + serviceZipCode);
                    }
                if (String.IsNullOrEmpty(billingStreetName) && String.IsNullOrEmpty(serviceStreetName))
                    {
                    throw new WRMNullValueException("A row has no address listed");
                    }
                if (String.IsNullOrEmpty(serviceStreetName))
                    {
                    serviceStreetNumber = commercialAccount.CommercialStreetNumber ?? 0;
                    serviceStreetName = commercialAccount.CommercialStreetName;
                    serviceUnitNumber = commercialAccount.CommercialUnitNumber;
                    serviceZipCode = commercialAccount.CommercialZipCode;
                    }
                // if service address is not filled in then use the billing address ?
                // WRMLogger.LogBuilder.AppendLine("Address table " + serviceStreetNumber + " " + serviceStreetName + " " + serviceUnitNumber);
                WRMLogger.Logger.log();
                Address address = new Address();
                int foundAddressId = 0;
                int addressId = 0;
                Address foundAddress = new Address();
                string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(serviceStreetName, serviceStreetNumber, serviceUnitNumber, serviceZipCode);
                // if address exists then use the address to create or update a new commercial account
                if (AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                    {
                    addressId = foundAddressId;
                    address = AddressPopulation.AddressDictionary[dictionaryKey];
                    address.AddressType = "COMMERCIAL";
                    if (!String.IsNullOrEmpty(recyclingServiceDay))
                        {
                        if (validateDayOfWeek(recyclingServiceDay))
                            {
                            address.RecycleDayOfWeek = recyclingServiceDay;
                            }
                        else
                            {
                            recyclingServiceDay = null;
                            }
                        }
                    if (!String.IsNullOrEmpty(recyclingServiceWeek))
                        {
                        if (validateRecycleFrequency(recyclingServiceWeek))
                            {
                            address.RecycleFrequency = recyclingServiceWeek;
                            }
                        else
                            {
                            recyclingServiceWeek = null;
                            }
                        }
                    if (!String.IsNullOrEmpty(trashServiceDay))
                        {
                        if (validateDayOfWeek(trashServiceDay))
                            {
                            address.TrashDayOfWeek = trashServiceDay;
                            }
                        else
                            {
                            address.AlternateSchedule = serviceDaysWeeks;
                            trashServiceDay = null;
                            }
                        }
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(address);

                    }
                else
                // if address does not exist then create one.  
                    {
                    address.StreetNumber = serviceStreetNumber;

                    address.StreetName = serviceStreetName;

                    address.UnitNumber = serviceUnitNumber;

                    address.ZipCode = serviceZipCode;
                    try
                        {
                        AddressPopulation.populateAddressFromKGIS(ref address);
                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR M1: Ignoring error " + dictionaryKey + " " + ex.Message);
                        }
                    address.AddressType = "COMMERCIAL";
                    if (!String.IsNullOrEmpty(recyclingServiceDay))
                        if (validateDayOfWeek(recyclingServiceDay))
                            {
                            address.RecycleDayOfWeek = recyclingServiceDay;
                            }
                        else
                            {
                            recyclingServiceDay = null;
                            }
                    if (!String.IsNullOrEmpty(recyclingServiceWeek))
                        {
                        if (validateRecycleFrequency(recyclingServiceWeek))
                            {
                            address.RecycleFrequency = recyclingServiceWeek;
                            }
                        else
                            {
                            recyclingServiceWeek = null;

                            }
                        }
                    if (!String.IsNullOrEmpty(trashServiceDay))
                        {
                        if (validateDayOfWeek(trashServiceDay))
                            {
                            address.TrashDayOfWeek = trashServiceDay;
                            }
                        else
                            {
                            address.AlternateSchedule = serviceDaysWeeks;
                            trashServiceDay = null;
                            }
                        }
                    if (!String.IsNullOrEmpty(addressAlternateSchedule))
                        {
                        address.AlternateSchedule = addressAlternateSchedule;
                        }
                    //WRMLogger.LogBuilder.AppendLine("Address table update Recycling Day Week =" + address.RecycleDayOfWeek + "/" + address.RecycleDayOfWeek + "- Trash Day =" + address.TrashDayOfWeek + "-");

                    address.NumberUnits = "1";
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);

                    if (String.IsNullOrEmpty(recyclingServiceDay))
                        {
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Entry<Address>(address).Property("RecycleDayOfWeek").IsModified = false;
                        }
                    if (String.IsNullOrEmpty(recyclingServiceWeek))
                        {
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Entry<Address>(address).Property("RecycleFrequency").IsModified = false;
                        }
                    if (String.IsNullOrEmpty(trashServiceDay))
                        {
                        WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Entry<Address>(address).Property("TrashDayOfWeek").IsModified = false;
                        }
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    AddressPopulation.AddressDictionary.Clear();
                    foreach (Address existingAddress in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                        {
                        /* WRMLogger.LogBuilder.AppendLine("Street " + existingAddress.StreetNumber + " " + existingAddress.StreetName + " " + existingAddress.UnitNumber + " " + existingAddress.ZipCode);
                         WRMLogger.Logger.log(); */
                        string existingDictionaryKey = IdentifierProvider.provideIdentifierFromAddress(existingAddress.StreetName, existingAddress.StreetNumber, existingAddress.UnitNumber, existingAddress.ZipCode);
                        AddressPopulation.AddressIdentiferDictionary[existingDictionaryKey] = existingAddress.AddressID;
                        AddressPopulation.ReverseAddressIdentiferDictionary[existingAddress.AddressID] = existingDictionaryKey;
                        AddressPopulation.AddressDictionary[existingDictionaryKey] = existingAddress;
                        }
                    }
                addressId = AddressPopulation.AddressIdentiferDictionary[dictionaryKey];
                Resident resident = new Resident();

                if (ResidentAddressPopulation.ResidentDictionary.TryGetValue(dictionaryKey, out resident))
                    {
                    updateResidentFromCommercialAccountRow(commercialAccountRow, ref resident);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(resident);
                    }
                else
                    {
                    resident = new Resident();
                    updateResidentFromCommercialAccountRow(commercialAccountRow, ref resident);

                    resident.AddressID = address.AddressID;
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(resident);
                    }
                commercialAccount.AddressID = addressId;
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(commercialAccount);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                }
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
            WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
            }
        private void updateResidentFromCommercialAccountRow(CommercialAccountRow commercialAccountRow, ref Resident resident)
            {
            resident.LastName = commercialAccountRow.PersonOfContact;
            resident.Phone = commercialAccountRow.ContactPhoneNumber;
            if (!String.IsNullOrEmpty(commercialAccountRow.ContactEmailAddress))
                {
                resident.Email = commercialAccountRow.ContactEmailAddress;
                resident.SendEmailNewsletter = true;
                }
            }

        public static CommercialAccount buildCommercialAccountFromAddress(Address address)
            {
            CommercialAccount commercialAccount = new CommercialAccount();
            commercialAccount.AddressID = address.AddressID;
            commercialAccount.CommercialStreetNumber = address.StreetNumber;
            commercialAccount.CommercialStreetName = address.StreetName;
            commercialAccount.CommercialUnitNumber = address.UnitNumber;
            commercialAccount.CommercialZipCode = address.ZipCode;
            commercialAccount.CommercialCity = "Knoxville";
            commercialAccount.CommercialState = "TN";
            commercialAccount.CommercialAccountStatus = "ACTIVE";


            return commercialAccount;
            }
        }
    }
