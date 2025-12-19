namespace PREACT.Fire
{
    public class FuelCoefficients
    {
        public string FuelType = string.Empty;
        public double q, BUI0, CrownBaseHeight, CrownFuelLoad;
        public int PercentConifer, PercentDeadFir;
        public double a, b, c;

        public FuelCoefficients()
        {

        }

        public FuelCoefficients(FuelCoefficients copyFrom)
        {
            if (copyFrom != null)
            {
                FuelType = copyFrom.FuelType;
                q = copyFrom.q;
                BUI0 = copyFrom.BUI0;
                CrownBaseHeight = copyFrom.CrownBaseHeight;
                CrownFuelLoad = copyFrom.CrownFuelLoad;
                PercentConifer = copyFrom.PercentConifer;
                PercentDeadFir = copyFrom.PercentDeadFir;
                a = copyFrom.a;
                b = copyFrom.b;
                c = copyFrom.c;
            }
        }
    }
}
