﻿using static WUIEngine.Fire.MathWrap;

namespace WUIEngine.Fire
{
    public class WindAdjustmentFactor
    {
        double windAdjustmentFactor_;
        double canopyCrownFraction_;
        WindAdjustmentFactorShelterMethod windAdjustmentFactorShelterMethod_;

        public WindAdjustmentFactor()
        {
            windAdjustmentFactor_ = 0.0;
            canopyCrownFraction_ = 0.0;
            windAdjustmentFactorShelterMethod_ = WindAdjustmentFactorShelterMethod.Unsheltered;
        }

        public double calculateWindAdjustmentFactorWithCrownRatio(double canopyCover, double canopyHeight,
            double crownRatio, double fuelbedDepth)
        {
            // Based on Albini and Baughman (1979)

            // canopyCrownFraction == fraction of the volume under the canopy top that is filled with
            // tree crowns (division by 3 assumes conical crown shapes).
            canopyCrownFraction_ = crownRatio * canopyCover / 3.0;

            calculateWindAdjustmentFactorShelterMethod(canopyCover, canopyHeight, fuelbedDepth);
            applyLogProfile(canopyCover, canopyHeight, fuelbedDepth);

            return windAdjustmentFactor_;
        }

        public double calculateWindAdjustmentFactorWithoutCrownRatio(double canopyCover, double canopyHeight, double fuelbedDepth)
        {
            // Based on Finney(1998, 2004)

            // canopyCrownFraction == fraction of the volume under the canopy top that is filled with tree crowns
            canopyCrownFraction_ = (canopyCover * M_PI) / 12.0; // RMRS-RP-4, eq. 45

            calculateWindAdjustmentFactorShelterMethod(canopyCover, canopyHeight, fuelbedDepth);
            applyLogProfile(canopyCover, canopyHeight, fuelbedDepth);

            return windAdjustmentFactor_;
        }

        public double getCanopyCrownFraction()
        {
            return canopyCrownFraction_;
        }

        public WindAdjustmentFactorShelterMethod getWindAdjustmentFactorShelterMethod()
        {
            return windAdjustmentFactorShelterMethod_;
        }

        public void calculateWindAdjustmentFactorShelterMethod(double canopyCover, double canopyHeight, double fuelbedDepth)
        {
            // Unsheltered
            if (canopyCover < 1.0e-07 || canopyCrownFraction_ < 0.05 || canopyHeight < 6.0)
            {
                windAdjustmentFactorShelterMethod_ = WindAdjustmentFactorShelterMethod.Unsheltered;
            }
            // Sheltered
            else
            {
                windAdjustmentFactorShelterMethod_ = WindAdjustmentFactorShelterMethod.Sheltered;
            }
        }

        public void applyLogProfile(double canopyCover, double canopyHeight, double fuelbedDepth)
        {
            if (windAdjustmentFactorShelterMethod_ == WindAdjustmentFactorShelterMethod.Unsheltered)
            {
                if (fuelbedDepth > 1.0e-07)
                {
                    windAdjustmentFactor_ = 1.83 / log((20.0 + 0.36 * fuelbedDepth) / (0.13 * fuelbedDepth));
                }
            }
            else // SHELTERED
            {
                windAdjustmentFactor_ = 0.555 / (sqrt(canopyCrownFraction_ * canopyHeight) * log((20.0 + 0.36 * canopyHeight) / (0.13 * canopyHeight)));
            }
        }
    }
}

