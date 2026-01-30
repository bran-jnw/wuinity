using System;
using System.Collections.Generic;
using static PREACT.Wildfire.FireWeatherIndex;
using System.IO;

namespace PREACT.Wildfire
{
    internal class FWICalculator
    {
        public static void CalculateYear(Year year, string outputName)
        {
            List<string> output = new List<string>();
            AddHeader(output);
            CalculateOneYear(year, output);
            SaveToFile(outputName, output);
        }

        /*public static void Calculate(YearCollection input, string outputName)
        {
            List<string> output = new List<string>();
            List<double> allFWIValues = new List<double>();
            StatisticalFWI statisticalFWI = new StatisticalFWI();
            AddHeader(output);

            for (int i = 0; i < input.years.Count; i++)
            {
                CalculateOneYear(input.years[i], output, allFWIValues, i == 0, statisticalFWI);
            }
            double mean = MathNet.Numerics.Statistics.Statistics.Mean(allFWIValues);
            double stdDev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(allFWIValues);
            double skewness = MathNet.Numerics.Statistics.Statistics.Skewness(allFWIValues);
            double variance = MathNet.Numerics.Statistics.Statistics.Variance(allFWIValues);
            //MonoBehaviour.print("Mean: " + mean + ", stDev: " + stdDev + ", skewness: " + skewness + ", variance: " + variance);
            SaveToFile(outputName, output);
            SaveSimpleFWI(outputName, input, allFWIValues);
            SaveFWIPercentiles(outputName, statisticalFWI);
        }*/

        static void AddHeader(List<string> output)
        {
            output.Add("DayIndex,Year,Month,Day,Fine Fuel Moisture Code,Duff Moisture Code,Drought Code,Initial Spread Index,Buildup Index,Fire Weather Index");
        }

        /*static void SaveSimpleFWI(string outputName, YearCollection yearCollection, List<double> allFwiValues)
        {
            List<string> output = new List<string>();
            string header = "DayIndex,";
            for (int i = 0; i < yearCollection.years.Count; i++)
            {
                header += "Year " + yearCollection.years[i].year;
                if (i < yearCollection.years.Count - 1)
                {
                    header += ",";
                }
            }
            output.Add(header);

            for (int i = 0; i < 365; i++)
            {
                string dataLine = "";
                for (int j = 0; j < yearCollection.years.Count; j++)
                {
                    if (j == 0)
                    {
                        dataLine += (i + 1) + ",";
                    }
                    dataLine += allFwiValues[j * 365 + i];
                    if (j < yearCollection.years.Count - 1)
                    {
                        dataLine += ",";
                    }
                }
                output.Add(dataLine);
            }

            File.WriteAllLines(Path.Combine(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "_output"), outputName + "_simple_fwi.csv"), output);
        }*/

        /*static void SaveFWIPercentiles(string outputName, StatisticalFWI statisticalFWI)
        {
            List<string> output = new List<string>();
            output.Add("DayIndex,25%-ile,50%-ile,75%-ile,95%-ile,99%-ile");
            for (int i = 0; i < 365; i++)
            {
                double twentyfive = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 25);
                double fifty = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 50);
                double seventyfive = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 75);
                double ninetyfive = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 95);
                double ninetynine = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 99);
                output.Add(i + "," + twentyfive + "," + fifty + "," + seventyfive + "," + ninetyfive + "," + ninetynine);
            }

            File.WriteAllLines(Path.Combine(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "_output"), outputName + "_fwi_percentiles.csv"), output);
        }*/

        static void SaveToFile(string outputFilePath, List<string> output)
        {
            File.WriteAllLines(outputFilePath, output);
        }

        static void CalculateOneYear(Year inputYear, List<string> output)
        {
            double temp, rhum, wind, prcp;
            double ffmc, ffmc0, dmc, dmc0, dc, dc0, isi, bui, fwi, ffwi, G, aPRCP;
            int year, month, day;
            // Initialize FMC, DMC, and DC
            ffmc0 = 85.0;
            dmc0 = 6.0;
            dc0 = 15.0;

            int dayIndex = 0;
            for (int j = 0; j < inputYear.months.Length; j++)
            {
                year = inputYear.year;
                for (int k = 0; k < inputYear.months[j].days.Length; k++)
                {
                    month = j + 1;
                    day = k + 1;
                    Day dayData = inputYear.months[j].days[k];
                    temp = dayData.tempMax;
                    rhum = dayData.rhumMin;
                    wind = dayData.windMax;
                    prcp = dayData.prcpMin;
                    FFMCcalc(temp, rhum, wind, prcp, ffmc0, out ffmc);
                    DMCcalc(temp, rhum, prcp, dmc0, month, out dmc);
                    DCcalc(temp, prcp, dc0, month, out dc);
                    ISIcalc(ffmc, wind, out isi);
                    BUIcalc(dmc, dc, out bui);
                    FWIcalc(isi, bui, out fwi);
                    ffmc0 = ffmc;
                    dmc0 = dmc;
                    dc0 = dc;

                    //do not count november - february
                    if (month < 2 || month > 9)
                    {
                        fwi = 0.0;
                    }

                    if (double.IsNaN(fwi))
                    {
                        fwi = 0.0;
                    }

                    output.Add(dayIndex + ", " + year + "," + month + "," + day + "," + ffmc + "," + dmc + "," + dc + "," + isi + "," + bui + "," + fwi);
                    ++dayIndex;
                }
            }
        }
    }
}
