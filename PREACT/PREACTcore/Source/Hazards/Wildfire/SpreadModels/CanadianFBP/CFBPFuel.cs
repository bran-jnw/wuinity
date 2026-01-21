namespace PREACT.Fire
{
    public class CFBPFuel
    {
        public string DescriptiveName;
        public CanadianFBP.FuelTypes FuelType;
        public FuelCoefficients Coefficients;
        public PREACTColor Color;

        public CFBPFuel(string fuel_type_column, string descriptiveName, PREACTColor color)
        {
            DescriptiveName = descriptiveName;

            if (fuel_type_column.StartsWith("C-1")) FuelType = CanadianFBP.FuelTypes.C1;
            else if (fuel_type_column.StartsWith("C-2")) FuelType = CanadianFBP.FuelTypes.C2;
            else if (fuel_type_column.StartsWith("C-3")) FuelType = CanadianFBP.FuelTypes.C3;
            else if (fuel_type_column.StartsWith("C-4")) FuelType = CanadianFBP.FuelTypes.C4;
            else if (fuel_type_column.StartsWith("C-5")) FuelType = CanadianFBP.FuelTypes.C5;
            else if (fuel_type_column.StartsWith("C-6")) FuelType = CanadianFBP.FuelTypes.C6;
            else if (fuel_type_column.StartsWith("C-7")) FuelType = CanadianFBP.FuelTypes.C7;

            else if (fuel_type_column.StartsWith("D-1")) FuelType = CanadianFBP.FuelTypes.D1;
            else if (fuel_type_column.StartsWith("D-2")) FuelType = CanadianFBP.FuelTypes.D2;

            else if (fuel_type_column.StartsWith("S-1")) FuelType = CanadianFBP.FuelTypes.S1;
            else if (fuel_type_column.StartsWith("S-2")) FuelType = CanadianFBP.FuelTypes.S2;
            else if (fuel_type_column.StartsWith("S-3")) FuelType = CanadianFBP.FuelTypes.S3;

            else if (fuel_type_column.StartsWith("O-1a")) FuelType = CanadianFBP.FuelTypes.O1a;
            else if (fuel_type_column.StartsWith("O-1b")) FuelType = CanadianFBP.FuelTypes.O1b;

            else if (fuel_type_column.StartsWith("M-1")) FuelType = CanadianFBP.FuelTypes.M1;
            else if (fuel_type_column.StartsWith("M-2")) FuelType = CanadianFBP.FuelTypes.M2;
            else if (fuel_type_column.StartsWith("M-3")) FuelType = CanadianFBP.FuelTypes.M3;
            else if (fuel_type_column.StartsWith("M-4")) FuelType = CanadianFBP.FuelTypes.M4;

            Coefficients = new FuelCoefficients(CanadianFBP.GetFuelCoefficients(nameof(FuelType)));

            //extra information that might be included
            int percent_conifer = 0, percent_dead_fir = 0;
            if (FuelType == CanadianFBP.FuelTypes.M1 || FuelType == CanadianFBP.FuelTypes.M2)
            {
                string[] split = fuel_type_column.Split('(');
                if (split.Length > 1)
                {
                    split = split[1].Split(' ');
                    if (int.TryParse(split[0], out percent_conifer)) ;
                }
            }
            Coefficients.PercentConifer = percent_conifer;

            if (FuelType == CanadianFBP.FuelTypes.M3 || FuelType == CanadianFBP.FuelTypes.M4)
            {
                string[] split = fuel_type_column.Split('(');
                if (split.Length > 1)
                {
                    split = split[1].Split(' ');
                    int.TryParse(split[0], out percent_dead_fir);
                }
            }
            Coefficients.PercentDeadFir = percent_dead_fir;
        }
    }
}
