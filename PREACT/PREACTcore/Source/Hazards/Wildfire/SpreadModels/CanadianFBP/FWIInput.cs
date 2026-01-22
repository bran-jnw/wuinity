using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public class Day
    {
        public float tempAverage, tempMin = float.MaxValue, tempMax = float.MinValue, tempCount, tempTotal;
        public float rhumAverage, rhumMin = float.MaxValue, rhumMax = float.MinValue, rhumCount, rhumTotal;
        public float windAverage, windMin = float.MaxValue, windMax = float.MinValue, windCount, windTotal;
        public float prcpAverage, prcpMin = float.MaxValue, prcpMax = float.MinValue, prcpCount, prcpTotal;
        public float flowAverage, flowMin = float.MaxValue, flowMax = float.MinValue, flowCount, flowTotal;
        public bool tempHasBeenSet = false, rhumHasBeenSet = false, windHasBeenSet = false, prcpHasBeenSet = false, flowHasBeenSet = false;
        public bool tempRepaired = false, rhumRepaired = false, windRepaired = false, prcpRepaired = false;
        [System.NonSerialized] public Month month;

        public Day(Month month)
        {
            this.month = month;
        }

        public void AddDayData(float temp, float rhum, float wind, float prcp)
        {
            AddTemperature(temp);
            AddRelativeHumidity(rhum);
            AddWind(wind);
            AddPrecipitationData(prcp);
        }

        public void AddDayData(float flow)
        {
            AddFlow(flow);
        }

        public void AddTemperature(float temp)
        {
            tempHasBeenSet = true;

            tempTotal += temp;
            ++tempCount;
            tempAverage = tempTotal / tempCount;
            if (temp < tempMin)
            {
                tempMin = temp;
            }
            if (temp > tempMax)
            {
                tempMax = temp;
            }
        }

        public void CopyTemperature(Day source)
        {
            if (tempHasBeenSet)
            {
                return;
            }

            tempHasBeenSet = true;
            tempRepaired = true;
            tempTotal = source.tempTotal;
            tempCount = source.tempCount;
            tempAverage = source.tempAverage;
            tempMin = source.tempMin;
            tempMax = source.tempMax;
        }

        public void AddRelativeHumidity(float rhum)
        {
            rhumHasBeenSet = true;

            rhumTotal += rhum;
            ++rhumCount;
            rhumAverage = rhumTotal / rhumCount;
            if (rhum < rhumMin)
            {
                rhumMin = rhum;
            }
            if (rhum > rhumMax)
            {
                rhumMax = rhum;
            }
        }

        public void CopyRelativeHumidity(Day source)
        {
            if (rhumHasBeenSet)
            {
                return;
            }

            rhumHasBeenSet = true;
            rhumRepaired = true;
            rhumTotal = source.rhumTotal;
            rhumCount = source.rhumCount;
            rhumAverage = source.rhumAverage;
            rhumMin = source.rhumMin;
            rhumMax = source.rhumMax;
        }

        public void AddWind(float wind)
        {
            windHasBeenSet = true;

            windTotal += wind;
            ++windCount;
            windAverage = windTotal / windCount;
            if (wind < windMin)
            {
                windMin = wind;
            }
            if (wind > windMax)
            {
                windMax = wind;
            }
        }

        public void CopyWind(Day source)
        {
            if (windHasBeenSet)
            {
                return;
            }

            windHasBeenSet = true;
            windRepaired = true;
            windTotal = source.windTotal;
            windCount = source.windCount;
            windAverage = source.windAverage;
            windMin = source.windMin;
            windMax = source.windMax;
        }

        public void AddPrecipitationData(float prcp)
        {
            prcpHasBeenSet = true;

            prcpTotal += prcp;
            ++prcpCount;
            prcpAverage = prcpTotal / prcpCount;
            if (prcp < prcpMin)
            {
                prcpMin = prcp;
            }
            if (prcp > prcpMax)
            {
                prcpMax = prcp;
            }
        }

        public void ResetPrecipitation()
        {
            prcpHasBeenSet = true;

            prcpTotal = 0f;
            prcpAverage = 0f;
            prcpMin = 0f;
            prcpMax = 0f;
        }

        public void SetPrecipitation(float newPrcp)
        {
            prcpTotal = newPrcp;
            prcpAverage = newPrcp;
            prcpCount = 1;
            prcpMin = newPrcp;
            prcpMax = newPrcp;
        }

        public void ScalePrcp(float ratio)
        {
            prcpTotal *= ratio;
            prcpAverage *= ratio;
            prcpMin *= ratio;
            prcpMax *= ratio;
        }

        public void CopyPrecipitation(Day source)
        {
            if (prcpHasBeenSet)
            {
                return;
            }

            prcpHasBeenSet = true;
            prcpRepaired = true;
            prcpTotal = source.prcpTotal;
            prcpCount = source.prcpCount;
            prcpAverage = source.prcpAverage;
            prcpMin = source.prcpMin;
            prcpMax = source.prcpMax;
        }

        public void AddFlow(float flow)
        {
            flowHasBeenSet = true;

            flowTotal += flow;
            ++flowCount;
            flowAverage = flowTotal / flowCount;
            if (flow < flowMin)
            {
                flowMin = flow;
            }
            if (flow > flowMax)
            {
                flowMax = flow;
            }
        }

        public bool HasCompleteData()
        {
            bool allSet = false;
            if (tempHasBeenSet && rhumHasBeenSet && windHasBeenSet && prcpHasBeenSet)
            {
                allSet = true;
            }
            else if (flowHasBeenSet)
            {
                allSet = true;
            }
            return allSet;
        }

        public void CopyData(Day source)
        {
            CopyTemperature(source);
            CopyRelativeHumidity(source);
            CopyWind(source);
            CopyPrecipitation(source);
        }

        public void ManipulateData(float deltaTemp, float deltaRhum, float deltaWind, float deltaPrcp)
        {
            tempAverage += deltaTemp;
            tempMin += deltaTemp;
            tempMax += deltaTemp;

            rhumAverage = Mathf.Clamp(rhumAverage + deltaRhum, 0.0f, 100.0f);
            rhumMin = Mathf.Clamp(rhumMin + deltaRhum, 0.0f, 100.0f);
            rhumMax = Mathf.Clamp(rhumMax + deltaRhum, 0.0f, 100.0f);

            windAverage = Mathf.Max(0.0f, windAverage + deltaWind);
            windMin = Mathf.Max(0.0f, windMin + deltaWind);
            windMax = Mathf.Max(0.0f, windMax + deltaWind);

            prcpAverage = Mathf.Max(0.0f, prcpAverage + deltaPrcp);
            prcpMin = Mathf.Max(0.0f, prcpMin + deltaPrcp);
            prcpMax = Mathf.Max(0.0f, prcpMax + deltaPrcp);
        }
    }

    [System.Serializable]
    public class Month
    {
        public Day[] days;
        public int monthIndex;

        public Month(Year year, int month)
        {
            monthIndex = month;
            int dayCount = GetNumberOfDays(month + 1, year.IsLeapYear());
            days = new Day[dayCount];
            for (int i = 0; i < dayCount; i++)
            {
                days[i] = new Day(this);
            }
        }

        public static int GetNumberOfDays(int month, bool isLeapYear)
        {
            int days = 30;
            switch (month)
            {
                case 1:
                    days = 31;
                    break;
                case 2:
                    if (isLeapYear)
                    {
                        days = 29;
                    }
                    else
                    {
                        days = 28;
                    }
                    break;
                case 3:
                    days = 31;
                    break;
                case 4:
                    days = 30;
                    break;
                case 5:
                    days = 31;
                    break;
                case 6:
                    days = 30;
                    break;
                case 7:
                    days = 31;
                    break;
                case 8:
                    days = 31;
                    break;
                case 9:
                    days = 30;
                    break;
                case 10:
                    days = 31;
                    break;
                case 11:
                    days = 30;
                    break;
                case 12:
                    days = 31;
                    break;
                default:
                    days = 30;
                    break;
            }
            return days;
        }
    }

    [System.Serializable]
    public class Year
    {
        public enum DataOfInterest { temp, prcp, wind, rhum, flow }
        public int year, invalidSequentialDataCount;
        public int minMonth = int.MaxValue, minDay = int.MaxValue, maxMonth = int.MinValue, maxDay = int.MinValue;
        public Month[] months;
        public Day[] simpleYear;
        public float totalPrecipitation = 0f;
        public int totalPrecipitationDays = 0;
        List<Day> daysOrderedByPrecipitation;
        List<int> prcpDaysInSequence;
        bool _IsLeapYear;

        public Year(int year, bool forceNoLeapYear)
        {
            this.year = year;
            if (forceNoLeapYear)
            {
                _IsLeapYear = false;
            }
            else
            {
                _IsLeapYear = IsLeapYear(year);
            }
            months = new Month[12];
            for (int i = 0; i < months.Length; i++)
            {
                months[i] = new Month(this, i);
            }

            int linearDayIndex = 0;
            simpleYear = new Day[365];
            for (int i = 0; i < months.Length; i++)
            {
                for (int j = 0; j < months[i].days.Length; j++)
                {
                    //leap year
                    if (months[i].days.Length == 29 && j == 28)
                    {
                        continue;
                    }
                    simpleYear[linearDayIndex] = months[i].days[j];
                    ++linearDayIndex;
                }
            }
        }

        //https://docs.microsoft.com/en-us/office/troubleshoot/excel/determine-a-leap-year
        static bool IsLeapYear(int year)
        {
            if (year % 4 == 0)
            {
                if (year % 100 == 0)
                {
                    if (year % 400 == 0)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsLeapYear()
        {
            return _IsLeapYear;
        }

        public void CollectNoPrcpDaysInSequence(out int maxDaysInSequence)
        {
            prcpDaysInSequence = new List<int>();

            int daysInRowWithoutPrcp = 0;
            maxDaysInSequence = int.MinValue;

            for (int i = 0; i < months.Length; i++)
            {
                for (int j = 0; j < months[i].days.Length; j++)
                {
                    if (months[i].days[j].prcpMin <= 0.0f)
                    {
                        ++daysInRowWithoutPrcp;
                    }
                    else
                    {
                        if (daysInRowWithoutPrcp > 0)
                        {
                            prcpDaysInSequence.Add(daysInRowWithoutPrcp);
                            if (daysInRowWithoutPrcp > maxDaysInSequence)
                            {
                                maxDaysInSequence = daysInRowWithoutPrcp;
                            }
                        }
                        daysInRowWithoutPrcp = 0;
                    }
                }
            }
        }

        public List<int> GetPrcpSequenceList()
        {

            if (prcpDaysInSequence == null)
            {
                int maxDaysInSequence;
                CollectNoPrcpDaysInSequence(out maxDaysInSequence);
            }
            return prcpDaysInSequence;
        }

        public void ManipulateData(float deltaTemp, float deltaRhum, float deltaWind, float deltaPrcp)
        {
            for (int i = 0; i < months.Length; i++)
            {
                for (int j = 0; j < months[i].days.Length; j++)
                {
                    months[i].days[j].ManipulateData(deltaTemp, deltaRhum, deltaWind, deltaPrcp);
                }
            }
        }

        public void ScalePrcp(float ratio)
        {
            for (int i = 0; i < months.Length; i++)
            {
                for (int j = 0; j < months[i].days.Length; j++)
                {
                    months[i].days[j].ScalePrcp(ratio);
                }
            }

            totalPrecipitation *= ratio;
        }

        public void EditYearlyPrcpByEditingDays(float delta_mm)
        {
            SortPrcpDays();

            if (delta_mm > 0)
            {
                //TODO, something with adding rain days when it is the most statistically likely
            }
            else
            {
                float prcpRemainder = -delta_mm;
                for (int i = 0; i < daysOrderedByPrecipitation.Count; i++)
                {
                    Day day = daysOrderedByPrecipitation[i]; //daysOrderedByPrecipitation.Count - 1 - i
                    if (day.month.monthIndex > 2 && day.month.monthIndex < 10)
                    {
                        if (prcpRemainder > day.prcpMin)
                        {
                            prcpRemainder -= day.prcpMin;
                            --totalPrecipitationDays;
                            totalPrecipitation -= day.prcpMin;
                            day.ResetPrecipitation();
                        }
                        else
                        {
                            //all out of changes
                            day.SetPrecipitation(prcpRemainder);
                            break;
                        }
                    }
                }
            }
        }


        public void ReduceRainDays(int numberOfDays)
        {
            SortPrcpDays();

            for (int i = 0; i < numberOfDays; i++)
            {
                //take from last positions
                daysOrderedByPrecipitation[daysOrderedByPrecipitation.Count - 1 - i].ResetPrecipitation();
            }
        }

        void SortPrcpDays()
        {
            if (daysOrderedByPrecipitation == null)
            {
                daysOrderedByPrecipitation = new List<Day>();
                for (int i = 0; i < months.Length; i++)
                {
                    for (int j = 0; j < months[i].days.Length; j++)
                    {
                        daysOrderedByPrecipitation.Add(months[i].days[j]);
                    }
                }

                daysOrderedByPrecipitation.Sort((x, y) => x.prcpMin.CompareTo(y.prcpMin));
            }
        }

        public void AddTemperature(float temp, int month, int day)
        {
            --month;
            --day;

            months[month].days[day].AddTemperature(temp);

            UpdateMaxMinMonthDay(month, day);
        }

        public void AddRelativeHumidity(float rhum, int month, int day)
        {
            --month;
            --day;

            months[month].days[day].AddRelativeHumidity(rhum);

            UpdateMaxMinMonthDay(month, day);
        }

        public void AddWind(float wind, int month, int day)
        {
            --month;
            --day;

            months[month].days[day].AddWind(wind);

            UpdateMaxMinMonthDay(month, day);
        }

        public void AddPrecipitation(float prcp, int month, int day)
        {
            --month;
            --day;

            months[month].days[day].AddPrecipitationData(prcp);

            if (prcp > 0f)
            {
                totalPrecipitation += prcp;
                ++totalPrecipitationDays;
            }

            UpdateMaxMinMonthDay(month, day);
        }

        public void AddFlow(float flow, int month, int day)
        {
            --month;
            --day;

            months[month].days[day].AddFlow(flow);

            UpdateMaxMinMonthDay(month, day);
        }

        public void AddDayData(int dayIndex, float temp, float rhum, float wind, float prcp)
        {
            simpleYear[dayIndex].AddDayData(temp, rhum, wind, prcp);
        }

        public void AddDayData(int dayIndex, float flow)
        {
            simpleYear[dayIndex].AddDayData(flow);
        }

        private void UpdateMaxMinMonthDay(int month, int day)
        {
            if (month < minMonth)
            {
                minMonth = month;
                if (day < minDay)
                {
                    minDay = day;
                }
            }

            if (month > maxMonth)
            {
                maxMonth = month;
                if (day > maxDay)
                {
                    maxDay = day;
                }
            }
        }

        public float GetDayInputData(int simpleYearIndex, DataOfInterest dataOfInterest)
        {
            float value = 0.0f;
            if (dataOfInterest == DataOfInterest.temp)
            {
                value = simpleYear[simpleYearIndex].tempMax;

            }
            else if (dataOfInterest == DataOfInterest.prcp)
            {
                value = simpleYear[simpleYearIndex].prcpMin;

            }
            else if (dataOfInterest == DataOfInterest.rhum)
            {
                value = simpleYear[simpleYearIndex].rhumMin;

            }
            else if (dataOfInterest == DataOfInterest.wind)
            {
                value = simpleYear[simpleYearIndex].windMax;

            }
            else if (dataOfInterest == DataOfInterest.flow)
            {
                value = simpleYear[simpleYearIndex].flowMax;

            }

            return value;
        }
    }
}
