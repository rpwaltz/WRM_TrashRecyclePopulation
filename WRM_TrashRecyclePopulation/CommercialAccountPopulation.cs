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

        private static Regex addressMatchRegex = new Regex(@"(\d+)?\s*([^,]+),?\s*(.*)");
        private static Regex serviceDaysMatchRegex = new Regex(@"([A-Z]+)(?:\/\s*([A-Z]+)\s*([AB])\s*WEEK)?");

        public static List<CommercialAccountRow> CommercialAccountRowList { get => commercialAccountRowList; set => commercialAccountRowList = value; }

        private static List<CommercialAccountRow> commercialAccountRowList = new List<CommercialAccountRow>();

        CommercialAccountImporter commercialAccountImporter = new CommercialAccountImporter();

        public CommercialAccountPopulation()
            {

            addActiveCommercialAccountFromWorksheetIntoDictionary(commercialAccountImporter.ActiveCommercialAccountWorksheet);
            addTerminatedCommercialAccountFromWorksheetIntoDictionary(commercialAccountImporter.TerminatedCommercialAccountWorksheet);
/*            addDowntownCrewCommercialAccountFromWorksheetIntoDictionary(commercialAccountImporter.DowntownPickupCrewCommercialAccountWorksheet); */
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
                commercialAccount.BillingNote = commercialAccountRow.BillingRate;
                
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

                if (String.IsNullOrEmpty(commercialAccountRow.BillingNote))
                    {
                    commercialAccount.BillingNote = commercialAccountRow.BillingNote;
                    }
                else
                    {
                    commercialAccount.BillingNote = commercialAccount.BillingNote + @"\n<br/>" + commercialAccountRow.BillingNote;
                    }

                if (String.IsNullOrEmpty(commercialAccountRow.AccountNotes))
                    {
                    commercialAccount.BillingNote = commercialAccountRow.AccountNotes;
                    }
                else
                    {
                    commercialAccount.BillingNote = commercialAccount.BillingNote + @"\n<br/>" + commercialAccountRow.AccountNotes;
                    }
                commercialAccount.CommercialAccountName = commercialAccountRow.CustomerName;
                commercialAccount.CommercialAccountNumber = commercialAccountRow.CustomerNumber;
                

                string streetName = null;
                int streetNumber = 0;
                string zipCode = null;
                string unitNumber = null;


                string serviceDaysWeeks = commercialAccountRow.ServiceDays;
                string trashServiceDay = null;
                string recyclingServiceDay = null;
                string recyclingServiceWeek = null;
                foreach (Match m in serviceDaysMatchRegex.Matches(serviceDaysWeeks))
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
                WRMLogger.LogBuilder.AppendLine("Recycling Day Week =" + recyclingServiceDay + "/" + recyclingServiceWeek + "- Trash Day =" + trashServiceDay + "-");
                
                string fullBillingStreetAddress = commercialAccountRow.BillingStreetNumber;
                foreach (Match m in addressMatchRegex.Matches(fullBillingStreetAddress))
                    {
                    if (m.Groups[1].Success)
                        {
                        streetNumber = int.Parse(m.Groups[1].Value);
                        }
                    if (m.Groups[2].Success)
                        {
                        streetName = m.Groups[2].Value;
                        }
                    if (m.Groups[3].Success)
                        {
                        unitNumber = m.Groups[3].Value;
                        }
                    commercialAccount.CommercialCity = commercialAccountRow.BillingCity;
                    commercialAccount.CommercialState =commercialAccountRow.BillingState;
                    commercialAccount.CommercialStreetNumber = streetNumber;
                    commercialAccount.CommercialStreetName = streetName;
                    commercialAccount.CommercialUnitNumber = unitNumber;
                    commercialAccount.CommercialZipCode = commercialAccountRow.BillingZipCode;
                    zipCode = commercialAccountRow.BillingZipCode;
                    }
                WRMLogger.LogBuilder.AppendLine("Billing " + streetNumber + " " + streetName + " " + unitNumber);
                WRMLogger.Logger.log();
                if (!String.IsNullOrEmpty(commercialAccountRow.ServiceAddress))
                    {
                    string streetAddress = commercialAccountRow.ServiceAddress;
                    foreach (Match m in addressMatchRegex.Matches(streetAddress))
                        {
                        if (m.Groups[1].Success)
                            {
                            streetNumber = int.Parse(m.Groups[1].Value);
                            }
                        if (m.Groups[2].Success)
                            {
                            streetName = m.Groups[2].Value;
                            }
                        if (m.Groups[3].Success)
                            {
                            unitNumber = m.Groups[3].Value;
                            }
                        zipCode = commercialAccountRow.ServiceZipCode;
                        }
                    }
                WRMLogger.LogBuilder.AppendLine("Street " + streetNumber + " " + streetName + " " + unitNumber); 
                WRMLogger.Logger.log();
                Address address = new Address();
                int foundAddressId = 0;
                int addressId = 0;
                Address foundAddress = new Address();
                string dictionaryKey = IdentifierProvider.provideIdentifierFromAddress(streetName, streetNumber, unitNumber, zipCode);
                // if address exists then use the address to create or update a new commercial account
                if (AddressPopulation.AddressIdentiferDictionary.TryGetValue(dictionaryKey, out foundAddressId))
                    {
                    addressId = foundAddressId;
                    address = AddressPopulation.AddressDictionary.GetValueOrDefault(dictionaryKey, null);
                    address.AddressType = "COMMERCIAL";
                    if (!String.IsNullOrEmpty(recyclingServiceDay))
                        {
                        address.RecycleDayOfWeek = recyclingServiceDay;
                        }
                    if (!String.IsNullOrEmpty(recyclingServiceWeek))
                        {
                        address.RecycleFrequency = recyclingServiceWeek;
                        }
                    if (!String.IsNullOrEmpty(trashServiceDay))
                        {
                        address.TrashDayOfWeek = trashServiceDay;
                        }
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Update(address);

                    }
                else
                // if address does not exist then create one.  
                    {
                    address.StreetNumber = streetNumber;

                    address.StreetName = streetName;

                    address.UnitNumber = unitNumber;

                    address.ZipCode = zipCode;
                    try
                        {
                        AddressPopulation.populateAddressFromKGIS(ref address);
                        }
                    catch (Exception ex)
                        {
                        WRMLogger.LogBuilder.AppendLine("ERROR: Ignoring error " + dictionaryKey + " " + ex.Message);
                        }
                    address.AddressType = "COMMERCIAL";
                    if (!String.IsNullOrEmpty(recyclingServiceDay))
                        {
                        address.RecycleDayOfWeek = recyclingServiceDay;
                        }
                    if (!String.IsNullOrEmpty(recyclingServiceWeek))
                        {
                        address.RecycleFrequency = recyclingServiceWeek;
                        }
                    if (!String.IsNullOrEmpty(trashServiceDay))
                        {
                        address.TrashDayOfWeek = trashServiceDay;
                        }
                    WRMLogger.LogBuilder.AppendLine("Recycling Day Week =" + address.RecycleDayOfWeek + "/" + address.RecycleDayOfWeek + "- Trash Day =" + address.TrashDayOfWeek + "-");

                    address.NumberUnits = "1";
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Add(address);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.SaveChanges(true);
                    WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.ChangeTracker.DetectChanges();
                    AddressPopulation.AddressDictionary.Clear();
                    foreach (Address existingAddress in WRM_EntityFrameworkContextCache.WrmTrashRecycleContext.Address.ToList())
                        {
                       /* WRMLogger.LogBuilder.AppendLine("Street " + existingAddress.StreetNumber + " " + existingAddress.StreetName + " " + existingAddress.UnitNumber + " " + existingAddress.ZipCode);
                        WRMLogger.Logger.log(); */
                        string existingDictionaryKey = IdentifierProvider.provideIdentifierFromAddress(existingAddress.StreetName, existingAddress.StreetNumber, existingAddress.UnitNumber, existingAddress.ZipCode);
                        AddressPopulation.AddressIdentiferDictionary[existingDictionaryKey] = address.AddressID;
                        AddressPopulation.ReverseAddressIdentiferDictionary[address.AddressID] = existingDictionaryKey;
                        AddressPopulation.AddressDictionary[existingDictionaryKey] = address;
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
        private void updateResidentFromCommercialAccountRow( CommercialAccountRow commercialAccountRow, ref Resident resident )
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
