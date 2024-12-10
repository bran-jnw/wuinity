//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform.Fire
{
    public static class BehaveUnits
    {
        public struct AreaUnits
        {
            public enum AreaUnitsEnum
            {
                SquareFeet, // base area unit
                Acres,
                Hectares,
                SquareMeters,
                SquareMiles,
                SquareKilometers
            }

            public static double toBaseUnits(double value, AreaUnitsEnum units)
            {
                const double ACRES_TO_SQUARE_FEET = 43560.002160576107;
                const double HECTARES_TO_SQUARE_FEET = 107639.10416709723;
                const double SQUARE_METERS_TO_SQUARE_FEET = 10.76391041671;
                const double SQUARE_MILES_TO_SQUARE_FEET = 27878400;
                const double SQUARE_KILOMETERS_TO_SQUARE_FEET = 10763910.416709721;

                switch (units)
                {
                    case AreaUnitsEnum.SquareFeet:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case AreaUnitsEnum.Acres:
                        {
                            value *= ACRES_TO_SQUARE_FEET;
                            break;
                        }
                    case AreaUnitsEnum.Hectares:
                        {
                            value *= HECTARES_TO_SQUARE_FEET;
                            break;
                        }
                    case AreaUnitsEnum.SquareMeters:
                        {
                            value *= SQUARE_METERS_TO_SQUARE_FEET;
                            break;
                        }
                    case AreaUnitsEnum.SquareMiles:
                        {
                            value *= SQUARE_MILES_TO_SQUARE_FEET;
                            break;
                        }
                    case AreaUnitsEnum.SquareKilometers:
                        {
                            value *= SQUARE_KILOMETERS_TO_SQUARE_FEET;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }

                return value;
            }

            public static double fromBaseUnits(double value, AreaUnitsEnum units)
            {
                const double SQUARE_FEET_TO_ACRES = 2.295684e-05;
                const double SQUARE_FEET_TO_HECTARES = 0.0000092903036;
                const double SQUARE_FEET_TO_SQUARE_KILOMETERS = 9.290304e-08;
                const double SQUARE_FEET_TO_SQUARE_METERS = 0.0929030353835;
                const double SQUARE_FEET_TO_SQUARE_MILES = 3.5870064279e-08;

                switch (units)
                {
                    case AreaUnitsEnum.SquareFeet:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case AreaUnitsEnum.Acres:
                        {
                            value *= SQUARE_FEET_TO_ACRES;
                            break;
                        }
                    case AreaUnitsEnum.Hectares:
                        {
                            value *= SQUARE_FEET_TO_HECTARES;
                            break;
                        }
                    case AreaUnitsEnum.SquareMeters:
                        {
                            value *= SQUARE_FEET_TO_SQUARE_METERS;
                            break;
                        }
                    case AreaUnitsEnum.SquareMiles:
                        {
                            value *= SQUARE_FEET_TO_SQUARE_MILES;
                            break;
                        }
                    case AreaUnitsEnum.SquareKilometers:
                        {
                            value *= SQUARE_FEET_TO_SQUARE_KILOMETERS;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
        }

        public struct LengthUnits
        {
            public enum LengthUnitsEnum
            {
                Feet, // base length unit
                Inches,
                Centimeters,
                Meters,
                Chains,
                Miles,
                Kilometers
            }

            public static double toBaseUnits(double value, LengthUnits.LengthUnitsEnum units)
            {
                // Length to base units constants
                const double INCHES_TO_FEET = 0.08333333333333;
                const double METERS_TO_FEET = 3.2808398950131;
                const double CENTIMETERS_TO_FEET = 0.03280839895;
                const double CHAINS_TO_FEET = 66.0;
                const double MILES_TO_FEET = 5280.0;
                const double KILOMETERS_TO_FEET = 3280.8398950131;

                switch (units)
                {
                    case LengthUnitsEnum.Feet:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case LengthUnitsEnum.Inches:
                        {
                            value *= INCHES_TO_FEET;
                            break;
                        }
                    case LengthUnitsEnum.Centimeters:
                        {
                            value *= CENTIMETERS_TO_FEET;
                            break;
                        }
                    case LengthUnitsEnum.Meters:
                        {
                            value *= METERS_TO_FEET;
                            break;
                        }
                    case LengthUnitsEnum.Chains:
                        {
                            value *= CHAINS_TO_FEET;
                            break;
                        }
                    case LengthUnitsEnum.Miles:
                        {
                            value *= MILES_TO_FEET;
                            break;
                        }
                    case LengthUnitsEnum.Kilometers:
                        {
                            value *= KILOMETERS_TO_FEET;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }

                }
                return value;
            }

            public static double fromBaseUnits(double value, LengthUnits.LengthUnitsEnum units)
            {
                // Length from base units constants
                const double FEET_TO_INCHES = 12;
                const double FEET_TO_CENTIMETERS = 30.480;
                const double FEET_TO_METERS = 0.3048;
                const double FEET_TO_CHAINS = 0.0151515151515;
                const double FEET_TO_MILES = 0.0001893939393939394;
                const double FEET_TO_KILOMETERS = 0.0003048;

                switch (units)
                {
                    case LengthUnitsEnum.Feet:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case LengthUnitsEnum.Inches:
                        {
                            value *= FEET_TO_INCHES;
                            break;
                        }
                    case LengthUnitsEnum.Centimeters:
                        {
                            value *= FEET_TO_CENTIMETERS;
                            break;
                        }
                    case LengthUnitsEnum.Meters:
                        {
                            value *= FEET_TO_METERS;
                            break;
                        }
                    case LengthUnitsEnum.Chains:
                        {
                            value *= FEET_TO_CHAINS;
                            break;
                        }
                    case LengthUnitsEnum.Miles:
                        {
                            value *= FEET_TO_MILES;
                            break;
                        }
                    case LengthUnitsEnum.Kilometers:
                        {
                            value *= FEET_TO_KILOMETERS;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
        }

        public struct LoadingUnits
        {
            public enum LoadingUnitsEnum
            {
                PoundsPerSquareFoot, // base loading unit
                TonsPerAcre,
                TonnesPerHectare,
                KilogramsPerSquareMeter
            }

            public static double toBaseUnits(double value, LoadingUnitsEnum units)
            {
                // Velocity to base units constants
                const double KILOGRAMS_PER_SQUARE_METER_TO_POUNDS_PER_SQUARE_FOOT = 0.2048161436225217;
                const double TONS_PER_ACRE_TO_POUNDS_PER_SQUARE_FOOT = 0.045913682277318638;
                const double TONNES_PER_HECTARE_TO_POUNDS_PER_SQUARE_FOOT = 0.02048161436225217;

                switch (units)
                {
                    case LoadingUnitsEnum.PoundsPerSquareFoot:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case LoadingUnitsEnum.TonsPerAcre:
                        {
                            value *= TONS_PER_ACRE_TO_POUNDS_PER_SQUARE_FOOT;
                            break;
                        }
                    case LoadingUnitsEnum.TonnesPerHectare:
                        {
                            value *= TONNES_PER_HECTARE_TO_POUNDS_PER_SQUARE_FOOT;
                            break;
                        }
                    case LoadingUnitsEnum.KilogramsPerSquareMeter:
                        {
                            value *= KILOGRAMS_PER_SQUARE_METER_TO_POUNDS_PER_SQUARE_FOOT;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
            
            public static double fromBaseUnits(double value, LoadingUnitsEnum units)
            {
                // Velocity to base units constants
                const double POUNDS_PER_SQUARE_FOOT_TO_KILOGRAMS_PER_SQUARE_METER = 4.88242763638305;
                const double POUNDS_PER_SQUARE_FOOT_TO_TONS_PER_ACRE = 21.78;
                const double POUNDS_PER_SQUARE_FOOT_TO_TONNES_PER_HECTARE = 48.8242763638305;

                switch (units)
                {
                    case LoadingUnitsEnum.PoundsPerSquareFoot:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case LoadingUnitsEnum.TonsPerAcre:
                        {
                            value *= POUNDS_PER_SQUARE_FOOT_TO_TONS_PER_ACRE;
                            break;
                        }
                    case LoadingUnitsEnum.TonnesPerHectare:
                        {
                            value *= POUNDS_PER_SQUARE_FOOT_TO_TONNES_PER_HECTARE;
                            break;
                        }
                    case LoadingUnitsEnum.KilogramsPerSquareMeter:
                        {
                            value *= POUNDS_PER_SQUARE_FOOT_TO_KILOGRAMS_PER_SQUARE_METER;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
            }

        public struct SurfaceAreaToVolumeUnits
        {
            public enum SurfaceAreaToVolumeUnitsEnum
            {
                SquareFeetOverCubicFeet, // base loading unit
                SquareMetersOverCubicMeters,
                SquareInchesOverCubicInches,
                SquareCentimetersOverCubicCentimers
            }

            public static double toBaseUnits(double value, SurfaceAreaToVolumeUnitsEnum units)
            {
                const double SQUARE_METERS_OVER_CUBIC_METERS_TO_SQUARE_FEET_OVER_CUBIC_FEET = 3.280839895013123;

                const double SQUARE_INCHES_OVER_CUBIC_INCHES_TO_SQUARE_FEET_OVER_CUBIC_FEET = 0.083333333333333;
                const double SQUARE_CENTIMETERS_OVER_CUBIC_CENTIMERS_TO_SQUARE_FEET_OVER_CUBIC_FEET = 0.03280839895013123;

                switch (units)
                {
                    case SurfaceAreaToVolumeUnitsEnum.SquareFeetOverCubicFeet:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case SurfaceAreaToVolumeUnitsEnum.SquareMetersOverCubicMeters:
                        {
                            value *= SQUARE_METERS_OVER_CUBIC_METERS_TO_SQUARE_FEET_OVER_CUBIC_FEET;
                            break;
                        }
                    case SurfaceAreaToVolumeUnitsEnum.SquareInchesOverCubicInches:
                        {
                            value *= SQUARE_INCHES_OVER_CUBIC_INCHES_TO_SQUARE_FEET_OVER_CUBIC_FEET;
                            break;
                        }
                    case SurfaceAreaToVolumeUnitsEnum.SquareCentimetersOverCubicCentimers:
                        {
                            value *= SQUARE_CENTIMETERS_OVER_CUBIC_CENTIMERS_TO_SQUARE_FEET_OVER_CUBIC_FEET;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
            public static double fromBaseUnits(double value, SurfaceAreaToVolumeUnitsEnum units)
            {
                const double SQUARE_FEET_OVER_CUBIC_FEET_TO_SQUARE_METERS_OVER_CUBIC_METERS = 0.3048;
                const double SQUARE_FEET_OVER_CUBIC_FEET_TO_SQUARE_INCHES_OVER_CUBIC_INCHES = 12;
                const double SQUARE_FEET_OVER_CUBIC_FEET_TO_SQUARE_CENTIMETERS_OVER_CUBIC_CENTIMERS = 30.48;

                switch (units)
                {
                    case SurfaceAreaToVolumeUnitsEnum.SquareFeetOverCubicFeet:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case SurfaceAreaToVolumeUnitsEnum.SquareMetersOverCubicMeters:
                        {
                            value *= SQUARE_FEET_OVER_CUBIC_FEET_TO_SQUARE_METERS_OVER_CUBIC_METERS;
                            break;
                        }
                    case SurfaceAreaToVolumeUnitsEnum.SquareInchesOverCubicInches:
                        {
                            value *= SQUARE_FEET_OVER_CUBIC_FEET_TO_SQUARE_INCHES_OVER_CUBIC_INCHES;
                            break;
                        }
                    case SurfaceAreaToVolumeUnitsEnum.SquareCentimetersOverCubicCentimers:
                        {
                            value *= SQUARE_FEET_OVER_CUBIC_FEET_TO_SQUARE_CENTIMETERS_OVER_CUBIC_CENTIMERS;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
        }

        public struct SpeedUnits
        {
            public enum SpeedUnitsEnum
            {
                FeetPerMinute, // base velocity unit
                ChainsPerHour,
                MetersPerSecond,
                MetersPerMinute,
                MilesPerHour,
                KilometersPerHour
            }

            public static double toBaseUnits(double value, SpeedUnits.SpeedUnitsEnum units)
            {
                // Velocity to base units constants
                const double METERS_PER_SECOND_TO_FEET_PER_MINUTE = 196.8503937;
                const double METERS_PER_MINUTE_TO_FEET_PER_MINUTE = 3.28084;
                const double CHAINS_PER_HOUR_TO_FEET_PER_MINUTE = 1.1;
                const double MILES_PER_HOUR_TO_FEET_PER_MINUTE = 88;
                const double KILOMETERS_PER_HOUR_TO_FEET_PER_MINUTE = 54.680665;

                switch (units)
                {
                    case SpeedUnitsEnum.FeetPerMinute:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case SpeedUnitsEnum.MetersPerSecond:
                        {
                            value *= METERS_PER_SECOND_TO_FEET_PER_MINUTE;
                            break;
                        }
                    case SpeedUnitsEnum.MetersPerMinute:
                        {
                            value *= METERS_PER_MINUTE_TO_FEET_PER_MINUTE;
                            break;
                        }
                    case SpeedUnitsEnum.ChainsPerHour:
                        {
                            value *= CHAINS_PER_HOUR_TO_FEET_PER_MINUTE;
                            break;
                        }
                    case SpeedUnitsEnum.MilesPerHour:
                        {
                            value *= MILES_PER_HOUR_TO_FEET_PER_MINUTE;
                            break;
                        }
                    case SpeedUnitsEnum.KilometersPerHour:
                        {
                            value *= KILOMETERS_PER_HOUR_TO_FEET_PER_MINUTE;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }

            public static double fromBaseUnits(double value, SpeedUnits.SpeedUnitsEnum units)
            {
                // Velocity from base units constants
                const double FEET_PER_MINUTE_TO_METERS_PER_SECOND = 0.00508;
                const double FEET_PER_MINUTE_TO_METERS_PER_MINUTE = 0.3048;
                const double FEET_PER_MINUTE_TO_CHAINS_PER_HOUR = 10.0 / 11.0;
                const double FEET_PER_MINUTE_TO_MILES_PER_HOUR = 0.01136363636;
                const double FEET_PER_MINUTE_TO_KILOMETERS_PER_HOUR = 0.018288;

                switch (units)
                {
                    case SpeedUnitsEnum.FeetPerMinute:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case SpeedUnitsEnum.MetersPerSecond:
                        {
                            value *= FEET_PER_MINUTE_TO_METERS_PER_SECOND;
                            break;
                        }
                    case SpeedUnitsEnum.MetersPerMinute:
                        {
                            value *= FEET_PER_MINUTE_TO_METERS_PER_MINUTE;
                            break;
                        }
                    case SpeedUnitsEnum.ChainsPerHour:
                        {
                            value *= FEET_PER_MINUTE_TO_CHAINS_PER_HOUR;
                            break;
                        }
                    case SpeedUnitsEnum.MilesPerHour:
                        {
                            value *= FEET_PER_MINUTE_TO_MILES_PER_HOUR;
                            break;
                        }
                    case SpeedUnitsEnum.KilometersPerHour:
                        {
                            value *= FEET_PER_MINUTE_TO_KILOMETERS_PER_HOUR;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
        }

        public struct CoverUnits
        {
            public enum CoverUnitsEnum
            {
                Fraction, // base cover unit
                Percent
            }

            public static double toBaseUnits(double value, CoverUnitsEnum units)
            {
                if (units == CoverUnitsEnum.Percent)
                {
                    value /= 100.0;
                }
                return value;
            }

                public static double fromBaseUnits(double value, CoverUnitsEnum units)
                {
                    if (units == CoverUnitsEnum.Percent)
                    {
                        value *= 100.0;
                    }
                    return value;
                }
            }

        public struct ProbabilityUnits
        {
            public enum ProbabilityUnitsEnum
            {
                Fraction, // base cover unit
                Percent
            }

            public static double toBaseUnits(double value, ProbabilityUnitsEnum units)
            {
                if (units == ProbabilityUnitsEnum.Percent)
                {
                    value /= 100.0;
                }
                return value;
            }

            public static double fromBaseUnits(double value, ProbabilityUnitsEnum units)
            {
                if (units == ProbabilityUnitsEnum.Percent)
                {
                    value *= 100.0;
                }
                return value;
            }
        }

        public struct MoistureUnits
        {
            public enum MoistureUnitsEnum
            {
                Fraction, // base cover unit
                Percent
            }

            public static double toBaseUnits(double value, MoistureUnitsEnum units)
            {
                if (units == MoistureUnitsEnum.Percent)
                {
                    value /= 100.0;
                }
                return value;
            }

            public static double fromBaseUnits(double value, MoistureUnitsEnum units)
            {
                if (units == MoistureUnitsEnum.Percent)
                {
                    value *= 100.0;
                }
                return value;
            }
        }

        public struct SlopeUnits
        {
            public enum SlopeUnitsEnum
            {
                Degrees, // base slope unit
                Percent
            }

            public static double toBaseUnits(double value, SlopeUnitsEnum units)
            {
                const double PI = 3.141592653589793238463;

                if (units == SlopeUnitsEnum.Percent)
                {
                    value = (180 / PI) * atan(value / 100.0); // slope is now in degees
                }
                return value;
            }

            public static double fromBaseUnits(double value, SlopeUnitsEnum units)
            {
                const double PI = 3.141592653589793238463;

                if (units == SlopeUnitsEnum.Percent)
                {
                    value = tan(value * (PI / 180)) * 100; // slope is now in percent
                }
                return value;
            }
        }

        public struct DensityUnits
        {
            public enum DensityUnitsEnum
            {
                PoundsPerCubicFoot, // base density unit
                KilogramsPerCubicMeter
            }

            public static double toBaseUnits(double value, DensityUnitsEnum units)
            {
                // Denisty to base units constants
                //public static const double KG_PER_CUBIC_METER_TO_LBS_PER_CUBIC_FOOT = 0.06242796051;
                const double KG_PER_CUBIC_METER_TO_LBS_PER_CUBIC_FOOT = 0.06242781786;

                if (units == DensityUnitsEnum.KilogramsPerCubicMeter)
                {
                    value *= KG_PER_CUBIC_METER_TO_LBS_PER_CUBIC_FOOT;
                }
                return value;
            }

            public static double fromBaseUnits(double value, DensityUnitsEnum units)
            {
                // Denisty from base units constants
                //public static const double LBS_PER_CUBIC_FOOT_TO_KG_PER_CUBIC_METER = 16.018463390932;
                const double LBS_PER_CUBIC_FOOT_TO_KG_PER_CUBIC_METER = 16.0185;

                if (units == DensityUnitsEnum.KilogramsPerCubicMeter)
                {
                    value *= LBS_PER_CUBIC_FOOT_TO_KG_PER_CUBIC_METER;
                }
                return value;
            }
        }

        public struct HeatOfCombustionUnits
        {
            public enum HeatOfCombustionUnitsEnum
            {
                BtusPerPound, // base heat of combustion unit
                KilojoulesPerKilogram
            }

            public static double toBaseUnits(double value, HeatOfCombustionUnitsEnum units)
            {
                const double KILOJOULES_PER_KILOGRAM_TO_BTUS_PER_POUND = 0.429592;

                switch (units)
                {
                    case HeatOfCombustionUnitsEnum.BtusPerPound:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case HeatOfCombustionUnitsEnum.KilojoulesPerKilogram:
                        {
                            value *= KILOJOULES_PER_KILOGRAM_TO_BTUS_PER_POUND;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }

                return value;
            }

            public static double fromBaseUnits(double value, HeatOfCombustionUnitsEnum units)
            {
                const double BTUS_PER_POUND_TO_KILOJOULES_PER_KILOGRAM = 2.32779;

                switch (units)
                {
                    case HeatOfCombustionUnitsEnum.BtusPerPound:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case HeatOfCombustionUnitsEnum.KilojoulesPerKilogram:
                        {
                            value *= BTUS_PER_POUND_TO_KILOJOULES_PER_KILOGRAM;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }

                return value;
            }
        }

        public struct HeatSinkUnits
        {
            public enum HeatSinkUnitsEnum
            {
                BtusPerCubicFoot, // base heat sink unit
                KilojoulesPerCubicMeter
            }

            public static double toBaseUnits(double value, HeatSinkUnitsEnum units)
            {
                const double KILOJOULES_PER_CUBIC_METER_TO_BTUS_PER_CUBIC_FOOT = 0.02681849745789;
                switch (units)
                {
                    case HeatSinkUnitsEnum.BtusPerCubicFoot:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case HeatSinkUnitsEnum.KilojoulesPerCubicMeter:
                        {
                            value *= KILOJOULES_PER_CUBIC_METER_TO_BTUS_PER_CUBIC_FOOT;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
            public static double fromBaseUnits(double value, HeatSinkUnitsEnum units)
            {
                const double BTUS_PER_CUBIC_FOOT_TO_KILOJOULES_PER_CUBIC_METER = 37.28769673134085;
                switch (units)
                {
                    case HeatSinkUnitsEnum.BtusPerCubicFoot:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case HeatSinkUnitsEnum.KilojoulesPerCubicMeter:
                        {
                            value *= BTUS_PER_CUBIC_FOOT_TO_KILOJOULES_PER_CUBIC_METER;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
        }

        //bran-jnw, not present in source
        /*public struct HeatPerUnitAreaUnits
        {
            public enum HeatPerUnitAreaUnitsEnum
            {
                BtusPerSquareFoot, // base reaction intensity unit
                KilojoulesPerSquareMeterPerSecond,
                KilowattsPerSquareMeter
            }

            public static double toBaseUnits(double value, HeatPerUnitAreaUnitsEnum units);
            public static double fromBaseUnits(double value, HeatPerUnitAreaUnitsEnum units);
        }*/

        public struct HeatSourceAndReactionIntensityUnits
        {
            public enum HeatSourceAndReactionIntensityUnitsEnum
            {
                BtusPerSquareFootPerMinute, // base reaction intensity unit
                BtusPerSquareFootPerSecond,
                KilojoulesPerSquareMeterPerSecond,
                KilojoulesPerSquareMeterPerMinute,
                KilowattsPerSquareMeter
            }

            public static double toBaseUnits(double value, HeatSourceAndReactionIntensityUnitsEnum units)
            {
                const double BTUS_PER_SQUARE_FOOT_PER_SECOND_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE = 60;
                const double KILOJOULES_PER_SQUARE_METER_PER_MINUTE_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE = 0.0880549963329497;
                //const double KILOWATTS_PER_SQUARE_METER_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE = 5.28329977997698;
                const double KILOWATTS_PER_SQUARE_METER_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE = 5.27921783108615;

                switch (units)
                {
                    case HeatSourceAndReactionIntensityUnitsEnum.BtusPerSquareFootPerMinute:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.BtusPerSquareFootPerSecond:
                        {
                            value *= BTUS_PER_SQUARE_FOOT_PER_SECOND_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE;
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.KilojoulesPerSquareMeterPerSecond:
                        {
                            value *= KILOWATTS_PER_SQUARE_METER_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE;
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.KilojoulesPerSquareMeterPerMinute:
                        {
                            value *= KILOJOULES_PER_SQUARE_METER_PER_MINUTE_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE;
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.KilowattsPerSquareMeter:
                        {
                            value *= KILOWATTS_PER_SQUARE_METER_TO_BTUS_PER_SQUARE_FOOT_PER_MINUTE;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }

                return value;
            }
            public static double fromBaseUnits(double value, HeatSourceAndReactionIntensityUnitsEnum units)
            {
                const double BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_BTUS_PER_SQUARE_FOOT_PER_SECOND = 0.01666666666666667;
                const double BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_KILOJOULES_PER_SQUARE_METER_PER_MINUTE = 11.356539;
                //const double BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_KILOWATTS_PER_SQUARE_METER = 0.18927565;
                const double BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_KILOWATTS_PER_SQUARE_METER = 0.189422;

                switch (units)
                {
                    case HeatSourceAndReactionIntensityUnitsEnum.BtusPerSquareFootPerMinute:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.BtusPerSquareFootPerSecond:
                        {
                            value *= BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_BTUS_PER_SQUARE_FOOT_PER_SECOND;
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.KilojoulesPerSquareMeterPerSecond:
                        {
                            value *= BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_KILOWATTS_PER_SQUARE_METER;
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.KilojoulesPerSquareMeterPerMinute:
                        {
                            value *= BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_KILOJOULES_PER_SQUARE_METER_PER_MINUTE;
                            break;
                        }
                    case HeatSourceAndReactionIntensityUnitsEnum.KilowattsPerSquareMeter:
                        {
                            value *= BTUS_PER_SQUARE_FOOT_PER_MINUTE_TO_KILOWATTS_PER_SQUARE_METER;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }

                return value;
            }
        }

        public struct FirelineIntensityUnits
        {
            public enum FirelineIntensityUnitsEnum
            {
                BtusPerFootPerSecond,  // base fireline intensity unit
                BtusPerFootPerMinute,
                KilojoulesPerMeterPerSecond,
                KilojoulesPerMeterPerMinute,
                KilowattsPerMeter
            }

            public static double toBaseUnits(double value, FirelineIntensityUnitsEnum units)
            {
                const double BTUS_PER_FOOT_PER_MINUTE_TO_BTUS_PER_FOOT_PER_SECOND = 0.01666666666666667;
                const double KILOJOULES_PER_METER_PER_MINUTE_TO_BTUS_PER_FOOT_PER_SECOND = 0.00481120819;
                const double KILOWATTS_PER_METER_TO_BTUS_PER_FOOT_PER_SECOND = 0.2886719;

                switch (units)
                {
                    case FirelineIntensityUnitsEnum.BtusPerFootPerSecond:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case FirelineIntensityUnitsEnum.BtusPerFootPerMinute:
                        {
                            value *= BTUS_PER_FOOT_PER_MINUTE_TO_BTUS_PER_FOOT_PER_SECOND;
                            break;
                        }
                    case FirelineIntensityUnitsEnum.KilojoulesPerMeterPerSecond:
                        {
                            value *= KILOWATTS_PER_METER_TO_BTUS_PER_FOOT_PER_SECOND;
                            break;
                        }
                    case FirelineIntensityUnitsEnum.KilojoulesPerMeterPerMinute:
                        {
                            value *= KILOJOULES_PER_METER_PER_MINUTE_TO_BTUS_PER_FOOT_PER_SECOND;
                            break;
                        }
                    case FirelineIntensityUnitsEnum.KilowattsPerMeter:
                        {
                            value *= KILOWATTS_PER_METER_TO_BTUS_PER_FOOT_PER_SECOND;
                            break;
                        }
                    default:
                        {
                            break ; // TODO: Handle error
                        }
                }

                return value;
            }

            public static double fromBaseUnits(double value, FirelineIntensityUnitsEnum units)
            {
                const double BTUS_PER_FOOT_PER_SECOND_TO_BTUS_PER_FOOT_PER_MINUTE = 60;
                const double BTUS_PER_FOOT_PER_SECOND_TO_KILOJOULES_PER_METER_PER_MINUTE = 207.848;
                const double BTUS_PER_FOOT_PER_SECOND_TO_KILOWATTS_PER_METER = 3.464140419;

                switch (units)
                {
                    case FirelineIntensityUnitsEnum.BtusPerFootPerSecond:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case FirelineIntensityUnitsEnum.BtusPerFootPerMinute:
                        {
                            value *= BTUS_PER_FOOT_PER_SECOND_TO_BTUS_PER_FOOT_PER_MINUTE;
                            break;
                        }
                    case FirelineIntensityUnitsEnum.KilojoulesPerMeterPerSecond:
                        {
                            value *= BTUS_PER_FOOT_PER_SECOND_TO_KILOWATTS_PER_METER;
                            break;
                        }
                    case FirelineIntensityUnitsEnum.KilojoulesPerMeterPerMinute:
                        {
                            value *= BTUS_PER_FOOT_PER_SECOND_TO_KILOJOULES_PER_METER_PER_MINUTE;
                            break;
                        }
                    case FirelineIntensityUnitsEnum.KilowattsPerMeter:
                        {
                            value *= BTUS_PER_FOOT_PER_SECOND_TO_KILOWATTS_PER_METER;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }

                return value;
            }
        }

        public struct TemperatureUnits
        {
            public enum TemperatureUnitsEnum
            {
                Fahrenheit, // base temperature unit
                Celsius,
                Kelvin
            }

            public static double toBaseUnits(double value, TemperatureUnitsEnum units)
            {
                switch (units)
                {
                    case TemperatureUnitsEnum.Fahrenheit:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case TemperatureUnitsEnum.Celsius:
                        {
                            value = ((value * 9.0) / 5.0) + 32;
                            break;
                        }
                    case TemperatureUnitsEnum.Kelvin:
                        {
                            value = (((value - 273.15) * 9.0) / 5.0) + 32;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }

            public static double fromBaseUnits(double value, TemperatureUnitsEnum units)
            {
                switch (units)
                {
                    case TemperatureUnitsEnum.Fahrenheit:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case TemperatureUnitsEnum.Celsius:
                        {
                            value = ((value - 32) * 5) / 9.0;
                            break;
                        }
                    case TemperatureUnitsEnum.Kelvin:
                        {
                            value = (((value - 32) * 5) / 9.0) + 273.15;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
        }

        public struct TimeUnits
        {
            public enum TimeUnitsEnum
            {
                Minutes, // base time unit
                Seconds,
                Hours
            }

            public static double toBaseUnits(double value, TimeUnitsEnum units)
            {
                switch (units)
                {
                    case TimeUnitsEnum.Minutes:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case TimeUnitsEnum.Seconds:
                        {
                            value /= 60.0;
                            break;
                        }
                    case TimeUnitsEnum.Hours:
                        {
                            value *= 60;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }

            public static double fromBaseUnits(double value, TimeUnitsEnum units)
            {
                switch (units)
                {
                    case TimeUnitsEnum.Minutes:
                        {
                            // Already in base, nothing to do
                            break;
                        }
                    case TimeUnitsEnum.Seconds:
                        {
                            value *= 60;
                            break;
                        }
                    case TimeUnitsEnum.Hours:
                        {
                            value /= 60.0;
                            break;
                        }
                    default:
                        {
                            break; // TODO: Handle error
                        }
                }
                return value;
            }
        }
    }
}

