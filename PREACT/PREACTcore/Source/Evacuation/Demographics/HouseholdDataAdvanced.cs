using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Evacuation
{
    public enum HouseholdType
    {
        single_wo_kids,
        single_w_kids_0_25,
        single_w_kids_25p,
        cohabit_wo_kids,
        cohabit_w_kids_0_25,
        cohabit_w_kids_25p,
        other_wo_kids,
        other_w_kids_0_25,
        other_w_kids_25p
    }

    public enum AgeBrackets { age_0_5, age_6_17, age_18_24, age_25_34, age_35_64, age_65_79, age_80p }
    public enum AdultAgeBrackets { age_18_24, age_25_34, age_35_64, age_65_79, age_80p }

    public enum ChildAgeBrackets { age_0_5, age_6_17, age_18_24, age_25p }

    public enum Employment { None, WorkAtWork, WorkFromHome}

    public abstract class HouseholdPerson
    {       
        public abstract bool HasDriversLicense();
    }

    public class Adult : HouseholdPerson
    {
        public AdultAgeBrackets ageBracket;
        public bool hasDriversLicense;
        public bool isDisabled;
        public Employment employment;

        public Adult(AdultAgeBrackets bracket, bool license, bool disabled, Employment employment) 
        {
            ageBracket = bracket;
            hasDriversLicense = license;
            isDisabled = disabled;
            this.employment = employment;

        }

        public override bool HasDriversLicense()
        {
            return hasDriversLicense;
        }
    }

    public class Child : HouseholdPerson
    {
        public ChildAgeBrackets ageBracket;

        public Child(ChildAgeBrackets ageBracket)
        {
            this.ageBracket = ageBracket;
        }

        public override bool HasDriversLicense()
        {
            throw new NotImplementedException();
        }
    }

    public class Toddler : Child
    {
        public bool inPreSchool;

        public Toddler(bool inPreSchool)
        {
            ageBracket = ChildAgeBrackets.age_0_5;
            this.inPreSchool = inPreSchool;
        }
    }

    public struct HouseholdDataAdvanced
    {
        public HouseholdType householdType;
        public int adultCount, childCount, vehicleCount;
        public HouseholdPerson[] people;

        public HouseholdDataAdvanced(HouseholdDemographicsAdvanced demographics)
        {
            householdType = demographics.GetHouseholdType();

            if((int)householdType < 3)
            {
                adultCount = 1;
            }
            else if((int)householdType < 6)
            {
                adultCount = 2;
            }
            else
            {
                //TODO, more complex
                adultCount = 2;
            }

            Adult[] adults = new Adult[adultCount];            
            //for now, assume all parents in same age bracket in "normal" households unless in collectives etc (fix this later with stats on age difference between adults in relation)
            AdultAgeBrackets adultAgeBracket = demographics.GetAdultAgeBracket(householdType);
            int employedCount = 0;
            for(int i = 0; i < adultCount; ++i)
            {
                //collectives etc we draw the ages independently
                if (householdType > HouseholdType.cohabit_w_kids_25p && i > 0)
                {
                    adultAgeBracket = demographics.GetAdultAgeBracket(householdType);
                }
                bool hasLicense = demographics.GetLicense(householdType, adultAgeBracket);
                bool hasDisability = demographics.GetDisability(householdType, adultAgeBracket);
                bool isEmployed = demographics.GetEmployment(householdType, adultAgeBracket);
                employedCount += isEmployed ? 1 : 0;
                Employment employment = isEmployed ? Employment.WorkAtWork : Employment.None;
                bool workingFromHome = demographics.GetWorkFromHome(householdType);
                employment = workingFromHome ? Employment.WorkFromHome : Employment.WorkAtWork;

                adults[i] = new Adult(adultAgeBracket, hasLicense, hasDisability, employment);
            }        


            childCount = demographics.GetChildCount(householdType);
            Child[] children = new Child[childCount];
            for (int i = childCount; i < childCount; ++i)
            {
                ChildAgeBrackets childAgeBracket = demographics.GetChildAgeBracket(householdType);
                if(childAgeBracket == ChildAgeBrackets.age_0_5)
                {
                    bool isInPreSchool = demographics.GetWorkFromHome(householdType);
                    children[i] = new Toddler(childAgeBracket, );
                }
                else
                {
                    if (childAgeBracket > ChildAgeBrackets.age_18_24)
                    {
                    }
                        children[i] = new Child(childAgeBracket);
                }    
            }

            people = new HouseholdPerson[adultCount + childCount];

            //we must consider the amount of children and emplyment here
            vehicleCount = demographics.GetVehicleCount(householdType);
        }
    }

    
}
