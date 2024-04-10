namespace WUIPlatform.Fire
{
    public class CellDeadFuelMoisture
    {
        double fuelMoisture_1, fuelMoisture_10, fuelMoisture_100, fuelMoisture_1000;
        DeadFuelMoisture dfm1h;
        DeadFuelMoisture dfm10h;
        DeadFuelMoisture dfm100h;
        DeadFuelMoisture dfm1000h;
        bool use1000Hour = false;
        FireCell cell;
        double latitude, longitude;

        public CellDeadFuelMoisture(FireCell cell, double latitude, double longitude, int startYear, int startMonth, int startDay, int startHour, bool use1000Hour = false)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.use1000Hour = use1000Hour;

            /*int startYear = 2000;
            int startMonth = 1;
            int startDay = 1;
            int startHour = 0;*/
            int startMinute = 0;
            int startSecond = 0;
            double startAirTemp = 20.0;
            double startAirHumidity = 50.0;            
            double startCumRain = 0.0;

            double stickTemp = startAirTemp;
            double stickSurfHumidity = startAirHumidity;


            InitialFuelMoisture startFuelMoisture = cell.GetMesh().initialFuelMoisture.GetInitialFuelMoisture(cell.GetFuelModelNumber());
            double stickMoisture1 = startFuelMoisture.OneHour;
            double stickMoisture10 = startFuelMoisture.TenHour;
            double stickMoisture100 = startFuelMoisture.HundredHour;

            long startDate = 0;
            long cloudCover = (long)cell.GetMesh().GetCurrentWindData().cloudCover;
            double startSolarRad = SunRadiation.SimpleRadiation(latitude, longitude, startDate, startHour, cloudCover, (long)cell.GetElevation(), (long)cell.GetSlope(), (long)cell.GetAspect(), cell.GetCanopyCover());

            dfm1h = DeadFuelMoisture.createDeadFuelMoisture1("stick_1hr");
            dfm10h = DeadFuelMoisture.createDeadFuelMoisture10("stick_10hr");
            dfm100h = DeadFuelMoisture.createDeadFuelMoisture100("stick_100hr");            

            //needed? happens in creation
            dfm1h.initializeStick();
            dfm10h.initializeStick();
            dfm100h.initializeStick();    

            //initialize the stick environment
            dfm1h.initializeEnvironment(startYear, startMonth, startDay, startHour, startMinute, startSecond,
                    startAirTemp, startAirHumidity, startSolarRad, startCumRain, stickTemp,
                    stickSurfHumidity, stickMoisture1);

            dfm10h.initializeEnvironment(startYear, startMonth, startDay, startHour, startMinute, startSecond,
                    startAirTemp, startAirHumidity, startSolarRad, startCumRain, stickTemp,
                    stickSurfHumidity, stickMoisture10);

            dfm100h.initializeEnvironment(startYear, startMonth, startDay, startHour, startMinute, startSecond,
                    startAirTemp, startAirHumidity, startSolarRad, startCumRain, stickTemp,
                    stickSurfHumidity, stickMoisture100);

            //get the stick moisture contents
            fuelMoisture_1 = dfm1h.meanWtdMoisture();
            fuelMoisture_10 = dfm10h.meanWtdMoisture();
            fuelMoisture_100 = dfm100h.meanWtdMoisture();

            if (use1000Hour)
            {
                dfm1000h = DeadFuelMoisture.createDeadFuelMoisture1000("stick_1000hr");
                dfm1000h.initializeStick();
                dfm1000h.initializeEnvironment(startYear, startMonth, startDay, startHour, startMinute, startSecond,
                    startAirTemp, startAirHumidity, startSolarRad, startCumRain, stickTemp,
                    stickSurfHumidity, stickMoisture100);
                fuelMoisture_1000 = dfm1000h.meanWtdMoisture();
            }
        }

        public void UpdateDeadFuelMoisture(int year, int month, int day, double airTemp, double airHumidity, double cumRain)
        {
            int Year = year;
            int Month = month;
            int Day = day;
            int Hour = 0;
            int Minute = 0;
            int Second = 0;
            double AirTemp = airTemp;
            double AirHumidity = airHumidity;
            double CumRain = cumRain;

            long Date = 0;
            long cloudCover = (long)cell.GetMesh().GetCurrentWindData().cloudCover;
            double SolarRad = SunRadiation.SimpleRadiation(latitude, longitude, Date, Hour, cloudCover, (long)cell.GetElevation(), (long)cell.GetSlope(), (long)cell.GetAspect(), cell.GetCanopyCover());

            dfm1h.update(Year, Month, Day, Hour, Minute, Second,
                 AirTemp, AirHumidity, SolarRad, CumRain);

            dfm10h.update(Year, Month, Day, Hour, Minute, Second,
                 AirTemp, AirHumidity, SolarRad, CumRain);

            dfm100h.update(Year, Month, Day, Hour, Minute, Second,
                 AirTemp, AirHumidity, SolarRad, CumRain);

            fuelMoisture_1 = dfm1h.meanWtdMoisture();
            fuelMoisture_10 = dfm10h.meanWtdMoisture();
            fuelMoisture_100 = dfm100h.meanWtdMoisture();

            if (use1000Hour)
            {
                dfm1000h.update(Year, Month, Day, Hour, Minute, Second,
                 AirTemp, AirHumidity, SolarRad, CumRain);
                fuelMoisture_1000 = dfm1000h.meanWtdMoisture();
            }            
        }

        public double getOneHour()
        {
            return fuelMoisture_1;
        }

        public double getTenHour()
        {
            return fuelMoisture_10;
        }

        public double getHundredHour()
        {
            return fuelMoisture_100;
        }

        public double getThousandHour()
        {
            if(!use1000Hour)
            {
                return 0.0;
            }

            return fuelMoisture_1000;
        }
    }
}

