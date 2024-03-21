/******************************************************************************
*
* Project:  CodeBlocks
* Purpose:  Class for calculating various properties related to fire size
* Author:   William Chatham <wchatham@fs.fed.us>
*
*******************************************************************************
*
* THIS SOFTWARE WAS DEVELOPED AT THE ROCKY MOUNTAIN RESEARCH STATION (RMRS)
* MISSOULA FIRE SCIENCES LABORATORY BY EMPLOYEES OF THE FEDERAL GOVERNMENT
* IN THE COURSE OF THEIR OFFICIAL DUTIES. PURSUANT TO TITLE 17 SECTION 105
* OF THE UNITED STATES CODE, THIS SOFTWARE IS NOT SUBJECT TO COPYRIGHT
* PROTECTION AND IS IN THE PUBLIC DOMAIN. RMRS MISSOULA FIRE SCIENCES
* LABORATORY ASSUMES NO RESPONSIBILITY WHATSOEVER FOR ITS USE BY OTHER
* PARTIES,  AND MAKES NO GUARANTEES, EXPRESSED OR IMPLIED, ABOUT ITS QUALITY,
* RELIABILITY, OR ANY OTHER CHARACTERISTIC.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
* DEALINGS IN THE SOFTWARE.
*
******************************************************************************/
using static WUInity.Fire.BehaveUnits;
using static WUInity.Fire.MathWrap;

namespace WUInity.Fire
{
    public class FireSize
    {
        // Inputs
        double effectiveWindSpeed_; // internally stored in mph
        double forwardSpreadRate_; // internally stored in ft/min
        double elapsedTime_; // internally stored in minutes

        // Outputs
        double ellipticalA_; // semi-minor axis of fire ellipse
        double ellipticalB_; // semi-major axis of fire ellipse
        double ellipticalC_; // distance from center of ellipse to a focus
        double eccentricity_; // measure of deviance from perfect circle, ranges from [0,1)
        double forwardSpreadDistance_;
        double backingSpreadRate_;
        double backingSpreadDistance_;
        double fireLengthToWidthRatio_;

        public FireSize()
        {

        }

        ~FireSize()
        {

        }

        public void calculateFireBasicDimensions(double effectiveWindSpeed, SpeedUnits.SpeedUnitsEnum windSpeedRateUnits, double forwardSpreadRate, SpeedUnits.SpeedUnitsEnum spreadRateUnits)
        {
            forwardSpreadRate_ = SpeedUnits.toBaseUnits(forwardSpreadRate, spreadRateUnits); // spread rate is now feet per minute
            if (windSpeedRateUnits != SpeedUnits.SpeedUnitsEnum.MilesPerHour)
            {
                effectiveWindSpeed_ = SpeedUnits.toBaseUnits(effectiveWindSpeed, windSpeedRateUnits); // wind speed is now feet per minute
                effectiveWindSpeed_ = SpeedUnits.fromBaseUnits(effectiveWindSpeed_, SpeedUnits.SpeedUnitsEnum.MilesPerHour); // wind speed is now miles per hour
            }
            else
            {
                effectiveWindSpeed_ = effectiveWindSpeed;
            }

            calculateFireLengthToWidthRatio();
            calculateSurfaceFireEccentricity();
            calculateBackingSpreadRate();

            calculateEllipticalDimensions();
        }

        public double getFireLengthToWidthRatio()
        {
            return fireLengthToWidthRatio_;
        }

        public double getEccentricity()
        {
            return eccentricity_;
        }

        public double getBackingSpreadRate(SpeedUnits.SpeedUnitsEnum spreadRateUnits)
        {
            return SpeedUnits.fromBaseUnits(backingSpreadRate_, spreadRateUnits);
        }

        public double getEllipticalA(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            elapsedTime = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
            return LengthUnits.fromBaseUnits((ellipticalA_* elapsedTime), lengthUnits);
        }

        public double getEllipticalB(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            elapsedTime = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
            return  LengthUnits.fromBaseUnits((ellipticalB_* elapsedTime), lengthUnits);
        }

        public double getEllipticalC(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            elapsedTime = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
            return LengthUnits.fromBaseUnits((ellipticalC_* elapsedTime), lengthUnits);
        }

        public double getFireLength(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            elapsedTime = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
            return LengthUnits.fromBaseUnits((ellipticalB_* elapsedTime * 2.0), lengthUnits);;
        }

        public double getMaxFireWidth(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            elapsedTime = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
            return LengthUnits.fromBaseUnits((ellipticalA_* elapsedTime * 2.0), lengthUnits);;
        }

        void calculateFireLengthToWidthRatio()
        {
            if (effectiveWindSpeed_ > 1.0e-07)
            {
                fireLengthToWidthRatio_ = 1.0 + (0.25 * effectiveWindSpeed_);
            }
            else
            {
                fireLengthToWidthRatio_ = 1.0;
            }
        }

        void calculateSurfaceFireEccentricity()
        {
            eccentricity_ = 0.0;
            double x = (fireLengthToWidthRatio_ * fireLengthToWidthRatio_) - 1.0;
            if (x > 0.0)
            {
                eccentricity_ = sqrt(x) / fireLengthToWidthRatio_;
            }
        }

        void calculateEllipticalDimensions()
        {
            ellipticalA_ = 0.0;
            ellipticalB_ = 0.0;
            ellipticalC_ = 0.0;

            // Internally A, B, and C are in terms of ft travelled in one minute
            ellipticalB_ = (forwardSpreadRate_ + backingSpreadRate_) / 2.0;
            if (fireLengthToWidthRatio_ > 1e-07)
            {
                ellipticalA_ = ellipticalB_ / fireLengthToWidthRatio_;
            }
            ellipticalC_ = ellipticalB_ - backingSpreadRate_;
        }

        void calculateBackingSpreadRate()
        {
            backingSpreadRate_ = forwardSpreadRate_ * (1.0 - eccentricity_) / (1.0 + eccentricity_);
        }

        public double getFirePerimeter(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            double perimeter = 0;
            elapsedTime = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
            double myEllipticalA = ellipticalA_ * elapsedTime;
            double myEllipticalB = ellipticalB_ * elapsedTime;
            if((myEllipticalA + myEllipticalB) > 1.0e-07)
            {
                double aMinusB = (myEllipticalA - myEllipticalB);
                double aMinusBSquared = aMinusB * aMinusB;
                double aPlusB = (myEllipticalA + myEllipticalB);
                double aPlusBSquared = aPlusB * aPlusB;
                double h = aMinusBSquared / aPlusBSquared;
                perimeter = M_PI* aPlusB * (1 + (h / 4.0) + ((h* h) / 64.0));
            } 
            return LengthUnits.fromBaseUnits(perimeter, lengthUnits);
        }

        public double getFireArea(AreaUnits.AreaUnitsEnum areaUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            elapsedTime = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
            return AreaUnits.fromBaseUnits(M_PI* ellipticalA_ * ellipticalB_ * elapsedTime * elapsedTime, areaUnits);
        }
    }
}

