namespace PREACT.Wildfire
{
    public class CanadianFBPFuel
    {
        public CanadianFBP.FuelTypes FuelType;
        public FuelCoefficients Coefficients;
        public PREACTColor Color;

        public CanadianFBPFuel(CanadianFBPLookupEntry lookupEntry)
        {
            if (lookupEntry.fuel_type.StartsWith("C-1")) FuelType = CanadianFBP.FuelTypes.C1;
            else if (lookupEntry.fuel_type.StartsWith("C-2")) FuelType = CanadianFBP.FuelTypes.C2;
            else if (lookupEntry.fuel_type.StartsWith("C-3")) FuelType = CanadianFBP.FuelTypes.C3;
            else if (lookupEntry.fuel_type.StartsWith("C-4")) FuelType = CanadianFBP.FuelTypes.C4;
            else if (lookupEntry.fuel_type.StartsWith("C-5")) FuelType = CanadianFBP.FuelTypes.C5;
            else if (lookupEntry.fuel_type.StartsWith("C-6")) FuelType = CanadianFBP.FuelTypes.C6;
            else if (lookupEntry.fuel_type.StartsWith("C-7")) FuelType = CanadianFBP.FuelTypes.C7;

            else if (lookupEntry.fuel_type.StartsWith("D-1")) FuelType = CanadianFBP.FuelTypes.D1;
            else if (lookupEntry.fuel_type.StartsWith("D-2")) FuelType = CanadianFBP.FuelTypes.D2;

            else if (lookupEntry.fuel_type.StartsWith("S-1")) FuelType = CanadianFBP.FuelTypes.S1;
            else if (lookupEntry.fuel_type.StartsWith("S-2")) FuelType = CanadianFBP.FuelTypes.S2;
            else if (lookupEntry.fuel_type.StartsWith("S-3")) FuelType = CanadianFBP.FuelTypes.S3;

            else if (lookupEntry.fuel_type.StartsWith("O-1a")) FuelType = CanadianFBP.FuelTypes.O1a;
            else if (lookupEntry.fuel_type.StartsWith("O-1b")) FuelType = CanadianFBP.FuelTypes.O1b;

            else if (lookupEntry.fuel_type.StartsWith("M-1")) FuelType = CanadianFBP.FuelTypes.M1;
            else if (lookupEntry.fuel_type.StartsWith("M-2")) FuelType = CanadianFBP.FuelTypes.M2;
            else if (lookupEntry.fuel_type.StartsWith("M-3")) FuelType = CanadianFBP.FuelTypes.M3;
            else if (lookupEntry.fuel_type.StartsWith("M-4")) FuelType = CanadianFBP.FuelTypes.M4;
            else FuelType = CanadianFBP.FuelTypes.NonFuel;

            Coefficients = new FuelCoefficients(CanadianFBP.GetFuelCoefficients(FuelType.ToString(), out bool success));

            if(!success || FuelType == CanadianFBP.FuelTypes.NonFuel)
            {
                FuelType = CanadianFBP.FuelTypes.NonFuel;
                return;
            }

            //extra information that might be included
            int percent_conifer = Coefficients.PercentConifer;
            if (FuelType == CanadianFBP.FuelTypes.M1 || FuelType == CanadianFBP.FuelTypes.M2)
            {
                string[] split = lookupEntry.fuel_type.Split('(');
                if (split.Length > 1)
                {
                    split = split[1].Split(' ');
                    int.TryParse(split[0], out percent_conifer);
                }
            }
            Coefficients.PercentConifer = percent_conifer;

            int percent_dead_fir =  Coefficients.PercentDeadFir;
            if (FuelType == CanadianFBP.FuelTypes.M3 || FuelType == CanadianFBP.FuelTypes.M4)
            {
                string[] split = lookupEntry.fuel_type.Split('(');
                if (split.Length > 1)
                {
                    split = split[1].Split(' ');
                    int.TryParse(split[0], out percent_dead_fir);
                }
            }
            Coefficients.PercentDeadFir = percent_dead_fir;

            Color = new PREACTColor(lookupEntry.r, lookupEntry.g, lookupEntry.b);
        }
    }
}
