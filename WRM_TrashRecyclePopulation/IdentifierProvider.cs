using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WRM_TrashRecyclePopulation
{
    class IdentifierProvider
        {
        string[] second = { "SECOND" };
        string[] weekDays2 = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        // static private Dictionary<string, string[]> 
        static public Dictionary<string, string[]> streetNumberConversionTable = new Dictionary<string, string[]>() {
             { "2ND", new string[] {"THIRD" } } ,
            { "3RD",    new string[] {"THIRD"} },
             { "4TH", new string[] { "FOURTH", "AVE"} },
             { "5TH" , new string[] { "FIFTH","AVE"} },
             { "6TH" , new string[] { "SIXTH"} },
             { "7TH" , new string[] { "SEVENTH"} },
             { "8TH" , new string[] { "EIGHTH","AVE"} },
             { "9TH" , new string[] { "NINTH", "AVE"} },
             { "11TH" , new string[]{ "ELEVENTH","ST"} },
             { "12TH" , new string[] { "TWELFTH","ST"} },
             { "13TH" , new string[] { "THIRTEENTH","ST"} },
             { "14TH" , new string[] { "FOURTEENTH", "ST"} },
             { "16TH" , new string[] { "SIXTEENTH", "ST"} },
             { "17TH" , new string[] { "SEVENTEENTH","ST"} },
             { "18TH" , new string[] { "EIGHTEENTH","ST"} },
             { "19TH" , new string[] { "NINETEENTH", "ST"} },
             { "20TH" , new string[] { "TWENTIETH","ST"} },
             { "21TH" , new string[] { "TWENTY FIRST","ST"} },
             { "22ND" , new string[] { "TWENTY SECOND","ST"} }};


        static public string provideIdentifierFromResident(string first_name, string last_name, string phone_number, string emailAddress)
            {
            string target = String.Format("{0}-{1}-{2}-{3}", first_name, last_name, phone_number, emailAddress);
            return target;
            }
        public static string provideIdentifierFromAddress (string streetName, int? streetNumber, string unitNumber, string zipCode)
        {
            string target = String.Format("{0}-{1}-{2}-{3}", normalizeStreetName(streetName).Replace(" ","-"), streetNumber, normalizeUnitNumber(unitNumber), normalizeZipCode(zipCode));
            return target;
        }
        public static string normalizeStreetName(string target)
            {
            if (string.IsNullOrEmpty(target))
                {
                throw new WRMNullValueException("Street Name is Empty");
                }
            target = target.Trim();
            target = target.ToUpper();
            target = normalizeWhiteSpaceInString(target);
            return target;
            }
            /*
            public static string normalizeStreetName(string target)
            {
                target = target.Trim();
                target = target.ToUpper();
                target = normalizeWhiteSpaceInString(target);
                Regex normalizeDirections = new Regex(@"(?:SOUTH|NORTH|WEST|EAST)\s+");
                MatchCollection directionMatches = normalizeDirections.Matches(target);
                if (directionMatches.Count > 0)
                    {

                    foreach (Match match in directionMatches) {
                        string fromString = match.Value;
                        string toString = match.Value.Substring(0, 1) + " ";
                        target = target.Replace(fromString, toString);
                        }
                    }
                Regex normalizeOrdinalValues = new Regex(@"([S|N|E|W])?\s?(\d{1,2}\s?(?:ST|ND|RD|TH))\s+(.+)");

                if (normalizeOrdinalValues.IsMatch(target))
                    {
                    MatchCollection normalizeOrdinalValueMatches = normalizeOrdinalValues.Matches(target);
                    Match normalizeOrdinalValueMatch = normalizeOrdinalValueMatches[0];
                    GroupCollection groupOrdinalValueMatch = normalizeOrdinalValueMatch.Groups;
                    string prefix = groupOrdinalValueMatch[1].ToString();
                    string numericOrdinal = groupOrdinalValueMatch[2].ToString();
                    string suffixAddress = groupOrdinalValueMatch[3].ToString();

                    if (streetNumberConversionTable.ContainsKey(numericOrdinal))
                        {
                        string[] ordinalValue = streetNumberConversionTable[numericOrdinal];
                        if (! String.IsNullOrEmpty(prefix))
                            {
                            prefix = prefix + " ";
                            }
                        if (ordinalValue.Length == 2)
                                {
                                target = prefix   + ordinalValue[0]  + " " + ordinalValue[1];
                                } 
                            else
                                {

                                target =  prefix + " " + ordinalValue[0];
                                }
                        }
                    else
                        {
                        throw new WRMNotSupportedException("Identity Provider: Cannot get " + numericOrdinal + " From streetNumberConversionTable");
                        }
                    }



                return target;
            }
            */
            public static int normalizeStreetNumber(string streetNumberString)
        {
            if (String.IsNullOrEmpty(streetNumberString))
                throw new WRMNullValueException("Street Number is Empty");
            int streetNumber;
            if (int.TryParse(streetNumberString, out streetNumber) )
                {
                return streetNumber;
                }
            else
                {
                throw new WRMNullValueException("Street Number is Malformed " + streetNumberString);
                }

            }
        public static string normalizeUnitNumber(string target)
        {
            if (String.IsNullOrEmpty(target))
                {
                return "";
                }
            target = target.Trim();
            target = target.ToUpper();
            target = normalizeWhiteSpaceInString(target);
            return target;
        }
        public static string normalizeZipCode(string target)
        {
            if (string.IsNullOrEmpty(target))
                {
                throw new WRMNullValueException("Zip Code is Empty");
                }
            target = target.Trim();
            target = target.ToUpper();
            target = normalizeWhiteSpaceInString(target);
            if (target.Length == 10)
            {
                if (target.Contains('-'))
                {
                    // determine if the +4 Code is 0000, if so remove it.
                    string fourPlus = target.Substring(6, 4);
                    if (fourPlus.Equals("0000"))
                    {
                        target = target.Substring(0, 5);
                    }
                } 
                else
                {
                    target = target.Substring(0, 5);
                }
            } else if (target.Length != 5)
            {
                throw new WRMNotSupportedException("Identifier Provider: Zip Code cannot be parsed " + target);
            }
            return target;
        }
        // https://stackoverflow.com/questions/6442421/c-sharp-fastest-way-to-remove-extra-white-spaces
        static string normalizeWhiteSpaceInString(string input)
        {
            int len = input.Length,
                index = 0,
                i = 0;
            var src = input.ToCharArray();
            bool skip = false;
            char ch;
            for (; i < len; i++)
            {
                ch = src[i];
                switch (ch)
                {
                    case '\u0020':
                    case '\u00A0':
                    case '\u1680':
                    case '\u2000':
                    case '\u2001':
                    case '\u2002':
                    case '\u2003':
                    case '\u2004':
                    case '\u2005':
                    case '\u2006':
                    case '\u2007':
                    case '\u2008':
                    case '\u2009':
                    case '\u200A':
                    case '\u202F':
                    case '\u205F':
                    case '\u3000':
                    case '\u2028':
                    case '\u2029':
                    case '\u0009':
                    case '\u000A':
                    case '\u000B':
                    case '\u000C':
                    case '\u000D':
                    case '\u0085':
                        if (skip) continue;
                        src[index++] = ch;
                        skip = true;
                        continue;
                    default:
                        skip = false;
                        src[index++] = ch;
                        continue;
                }
            }

            return new string(src, 0, index);
        }
    }
}
