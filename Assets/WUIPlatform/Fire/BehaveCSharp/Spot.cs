using static WUIPlatform.Fire.BehaveUnits;
using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform.Fire
{
    public class Spot
    {
        SpotInputs spotInputs_;

        double[][] speciesFlameHeightParameters_;// = new double[(int)SpotInputs.SpotArrayConstants.SpotArrayConstantsEnum.NUM_SPECIES][(int)SpotInputs.SpotArrayConstants.SpotArrayConstantsEnum.NUM_COLS];
        double[][] speciesFlameDurationParameters_;// = new double[(int)SpotInputs.SpotArrayConstants.SpotArrayConstantsEnum.NUM_SPECIES][(int)SpotInputs.SpotArrayConstants.SpotArrayConstantsEnum.NUM_COLS];
        double[][] firebrandHeightFactors_;// = new double[(int)SpotInputs.SpotArrayConstants.SpotArrayConstantsEnum.NUM_SPECIES][(int)SpotInputs.SpotArrayConstants.SpotArrayConstantsEnum.NUM_COLS];

        // Outputs
        double coverHeightUsedForSurfaceFire_;      // Actual tree / vegetation ht used for surface fire(ft)
        double coverHeightUsedForBurningPile_;      // Actual tree / vegetation ht used for burning pile(ft)
        double coverHeightUsedForTorchingTrees_;    // Actual tree / vegetation ht used for burning pile(ft)
        double flameHeightForTorchingTrees_;        // Steady state flame height for torching trees(ft).
        double flameRatio_;                         // Ratio of tree height to steady flame height(ft / ft).
        double firebrandDrift_;                     // Maximum firebrand drift from surface fire(mi).
        double flameDuration_;                      // Flame duration(dimensionless)
        double firebrandHeightFromBurningPile_;     // Initial maximum firebrand height for burning pile(ft).
        double firebrandHeightFromSurfaceFire_;     // Initial maximum firebrand height for surface fire(ft).
        double firebrandHeightFromTorchingTrees_;   // Initial maximum firebrand height for torching trees(ft).
        double flatDistanceFromBurningPile_;        // Maximum spotting distance over flat terrain for burning pile(mi).
        double flatDistanceFromSurfaceFire_;        // Maximum spotting distance over flat terrain for surface fire(mi).
        double flatDistanceFromTorchingTrees_;      // Maximum spotting distance over flat terrain for torching trees(mi).
        double mountainDistanceFromBurningPile_;    // Maximum spotting distance over mountain terrain for burning pile(mi).
        double mountainDistanceFromSurfaceFire_;    // Maximum spotting distance over mountain terrain surface fire(mi).
        double mountainDistanceFromTorchingTrees_;  // Maximum spotting distance over mountain terrain torching trees(mi).

        public Spot()
        {
            initializeMembers();
        }

        ~Spot()
        {

        }

        public Spot(Spot rhs)
        {
            memberwiseCopyAssignment(rhs);
        }

        /*Spot & operator=(const Spot& rhs)
        {
            if (this != &rhs)
            {
                memberwiseCopyAssignment(rhs);
            }
            return *this;
        }*/

        void memberwiseCopyAssignment(Spot rhs)
        {
            /*memcpy(speciesFlameHeightParameters_, rhs.speciesFlameHeightParameters_, SpotInputs.SpotArrayConstants.NUM_SPECIES * sizeof(speciesFlameHeightParameters_[0]));
            memcpy(speciesFlameDurationParameters_, rhs.speciesFlameDurationParameters_, SpotInputs.SpotArrayConstants.NUM_SPECIES * sizeof(speciesFlameDurationParameters_[0]));
            memcpy(firebrandHeightFactors_, rhs.firebrandHeightFactors_, SpotInputs.SpotArrayConstants.NUM_FIREBRAND_ROWS * sizeof(firebrandHeightFactors_[0]));*/
            //TODO: faster copy, e.g: https://stackoverflow.com/questions/4670720/extremely-fast-way-to-clone-the-values-of-a-jagged-array-into-a-second-array
            speciesFlameHeightParameters_ = new double[rhs.speciesFlameHeightParameters_.Length][];
            for (int i = 0; i < rhs.speciesFlameHeightParameters_.Length; i++)
            {
                speciesFlameHeightParameters_[i] = new double[rhs.speciesFlameHeightParameters_[i].Length];
                for (int j = 0; j < rhs.speciesFlameHeightParameters_[i].Length; j++)
                {
                    speciesFlameHeightParameters_[i][j] = rhs.speciesFlameHeightParameters_[i][j];
                }
            }

            speciesFlameDurationParameters_ = new double[rhs.speciesFlameDurationParameters_.Length][];
            for (int i = 0; i < rhs.speciesFlameDurationParameters_.Length; i++)
            {
                speciesFlameDurationParameters_[i] = new double[rhs.speciesFlameDurationParameters_[i].Length];
                for (int j = 0; j < rhs.speciesFlameDurationParameters_[i].Length; j++)
                {
                    speciesFlameDurationParameters_[i][j] = rhs.speciesFlameDurationParameters_[i][j];
                }
            }

            firebrandHeightFactors_ = new double[rhs.firebrandHeightFactors_.Length][];
            for (int i = 0; i < rhs.firebrandHeightFactors_.Length; i++)
            {
                firebrandHeightFactors_[i] = new double[rhs.firebrandHeightFactors_[i].Length];
                for (int j = 0; j < rhs.firebrandHeightFactors_[i].Length; j++)
                {
                    firebrandHeightFactors_[i][j] = rhs.firebrandHeightFactors_[i][j];
                }
            }

            coverHeightUsedForSurfaceFire_ = rhs.coverHeightUsedForSurfaceFire_;
            coverHeightUsedForBurningPile_ = rhs.coverHeightUsedForBurningPile_;
            coverHeightUsedForTorchingTrees_ = rhs.coverHeightUsedForTorchingTrees_;
            flameHeightForTorchingTrees_ = rhs.flameHeightForTorchingTrees_;
            flameRatio_ = rhs.flameRatio_;
            firebrandDrift_ = rhs.firebrandDrift_;
            flameDuration_ = rhs.flameDuration_;
            firebrandHeightFromBurningPile_ = rhs.firebrandHeightFromBurningPile_;
            firebrandHeightFromSurfaceFire_ = rhs.firebrandHeightFromSurfaceFire_;
            firebrandHeightFromTorchingTrees_ = rhs.firebrandHeightFromTorchingTrees_;
            flatDistanceFromBurningPile_ = rhs.flatDistanceFromBurningPile_;
            flatDistanceFromSurfaceFire_ = rhs.flatDistanceFromSurfaceFire_;
            flatDistanceFromTorchingTrees_ = rhs.mountainDistanceFromBurningPile_;
            mountainDistanceFromBurningPile_ = rhs.mountainDistanceFromBurningPile_;
            mountainDistanceFromSurfaceFire_ = rhs.mountainDistanceFromBurningPile_;
            mountainDistanceFromTorchingTrees_ = rhs.mountainDistanceFromBurningPile_;
        }

        // Set up speciesFlameDurationParameters_
        
        void initializeMembers()
        {
            // Set up speciesFlameHeightParameters_
            speciesFlameHeightParameters_ = new double[][]
            {
                new double[] { 15.7, 0.451 },  //  0 Engelmann spruce
		        new double[] { 15.7, 0.451 },  //  1 Douglas-fir
		        new double[] { 15.7, 0.451 },  //  2 subalpine fir
		        new double[] { 15.7, 0.451 },  //  3 western hemlock
		        new double[] { 12.9, 0.453 },  //  4 ponderosa pine
		        new double[] { 12.9, 0.453 },  //  5 lodgepole pine
		        new double[] { 12.9, 0.453 },  //  6 western white pine
		        new double[] { 16.5, 0.515 },  //  7 grand fir
		        new double[] { 16.5, 0.515 },  //  8 balsam fir
		        new double[] { 2.71, 1.000 },  //  9 slash pine
		        new double[] { 2.71, 1.000 },  // 10 longleaf pine
		        new double[] { 2.71, 1.000 },  // 11 pond pine
		        new double[] { 2.71, 1.000 },  // 12 shortleaf pine
		        new double[] { 2.71, 1.000 }   // 13 loblolly pine
                //{12.9, .453 },  // 14 western larch (guessed)
                //{15.7, .515 }   // 15 western red cedar (guessed)
            };
            //memcpy(speciesFlameHeightParameters_, tempSpeciesFlameHeightParameters, SpotInputs.SpotArrayConstants.NUM_SPECIES * sizeof(speciesFlameHeightParameters_[0]));

            // Set up speciesFlameDurationParameters_
            speciesFlameDurationParameters_ = new double[][]
            {
                new double[] { 12.6, -0.256 },  //  0 Engelmann spruce
		        new double[] { 10.7, -0.278 },  //  1 Douglas-fir
		        new double[] { 10.7, -0.278 },  //  2 subalpine fir
		        new double[] { 6.30, -0.249 },  //  3 western hemlock
		        new double[] { 12.6, -0.256 },  //  4 ponderosa pine
		        new double[] { 12.6, -0.256 },  //  5 lodgepole pine
		        new double[] { 10.7, -0.278 },  //  6 western white pine
		        new double[] { 10.7, -0.278 },  //  7 grand fir
		        new double[] { 10.7, -0.278 },  //  8 balsam fir
		        new double[] { 11.9, -0.389 },  //  9 slash pine
		        new double[] { 11.9, -0.389 },  // 10 longleaf pine
		        new double[] { 7.91, -0.344 },  // 11 pond pine
		        new double[] { 7.91, -0.344 },  // 12 shortleaf pine
		        new double[] { 13.5, -0.544 }   // 13 loblolly pine
                //{ 6.3, -.249},   // 14 western larch (guessed)
                //{ 12.6, -.256}   // 15 western red cedar (guessed)
            };
            //(speciesFlameDurationParameters_, tempSpeciesFlameDurationParameters, SpotInputs.SpotArrayConstants.NUM_SPECIES * sizeof(speciesFlameDurationParameters_[0]));

            // Set up firebrandHeightFactors_
            firebrandHeightFactors_ = new double[][]
            {
                new double[]{ 4.24, 0.332 },
                new double[]{ 3.64, 0.391 },
                new double[]{ 2.78, 0.418 },
                new double[]{ 4.70, 0.000 }
            };
            //memcpy(firebrandHeightFactors_, tempFirebrandHeightFactors, SpotInputs.SpotArrayConstants.NUM_FIREBRAND_ROWS * sizeof(firebrandHeightFactors_[0]));

            coverHeightUsedForSurfaceFire_ = 0.0;
            coverHeightUsedForBurningPile_ = 0.0;
            coverHeightUsedForTorchingTrees_ = 0.0;
            flameHeightForTorchingTrees_ = 0.0;
            flameRatio_ = 0.0;
            firebrandDrift_ = 0.0;
            flameDuration_ = 0.0;
            firebrandHeightFromBurningPile_ = 0.0;
            firebrandHeightFromSurfaceFire_ = 0.0;
            firebrandHeightFromTorchingTrees_ = 0.0;
            flatDistanceFromBurningPile_ = 0.0;
            flatDistanceFromSurfaceFire_ = 0.0;
            flatDistanceFromTorchingTrees_ = 0.0;
            mountainDistanceFromBurningPile_ = 0.0;
            mountainDistanceFromSurfaceFire_ = 0.0;
            mountainDistanceFromTorchingTrees_ = 0.0;

            //bran-jnw
            spotInputs_ = new SpotInputs();
        }

        double calculateSpotCriticalCoverHeight(double firebrandHeight, double coverHeight)
        {
            // Minimum value of coverHeight used to calculate flatDistance
            // using log variation with ht.
            double criticalHeight = (firebrandHeight < 1e-7)
                ? (0.0)
                : (2.2 * pow(firebrandHeight, 0.337) - 4.0);

            // Cover height used in calculation of flatDistance.
            double coverHeightUsed = (coverHeight > criticalHeight)
                ? (coverHeight)
                : (criticalHeight);

            return coverHeightUsed;
        }

        double spotDistanceMountainTerrain(double flatDistance, SpotFireLocation.SpotFireLocationEnum location, double ridgeToValleyDistance, double ridgeToValleyElevation)
        {
            double mountainDistance = flatDistance;
            if (ridgeToValleyElevation > 1e-7 && ridgeToValleyDistance > 1e-7)
            {
                double a1 = flatDistance / ridgeToValleyDistance;
                double b1 = ridgeToValleyElevation / (10.0 * M_PI) / 1000.0;
                double x = a1;
                for (int i = 0; i < 6; i++)
                {
                    x = a1 - b1 * (cos(M_PI * x - (int)location * M_PI / 2.0)
                        - cos((int)location * M_PI / 2.0));
                }
                mountainDistance = x * ridgeToValleyDistance;
            }
            return mountainDistance;
        }

        double spotDistanceFlatTerrain(double firebrandHeight, double coverHeight, double windSpeedAtTwentyFeet)
        {
            // Flat terrain spotting distance.
            double flatDistance = 0.0;
            if (coverHeight > 1e-7)
            {
                flatDistance = 0.000718 * windSpeedAtTwentyFeet * sqrt(coverHeight)
                    * (0.362 + sqrt(firebrandHeight / coverHeight) / 2.0
                        * log(firebrandHeight / coverHeight));
            }
            return flatDistance;
        }

        void calculateSpottingDistanceFromBurningPile()
        {
            // Get needed inputs
            SpotFireLocation.SpotFireLocationEnum location = spotInputs_.getLocation();
            double ridgeToValleyDistance = spotInputs_.getRidgeToValleyDistance(LengthUnits.LengthUnitsEnum.Miles);
            double ridgeToValleyElevation = spotInputs_.getRidgeToValleyElevation(LengthUnits.LengthUnitsEnum.Feet);
            double downwindCoverHeight = spotInputs_.getDownwindCoverHeight(LengthUnits.LengthUnitsEnum.Feet);
            double windSpeedAtTwentyFeet = spotInputs_.getWindSpeedAtTwentyFeet(SpeedUnits.SpeedUnitsEnum.MilesPerHour);
            double burningPileflameHeight = spotInputs_.getBurningPileFlameHeight(LengthUnits.LengthUnitsEnum.Feet);

            // Initialize return values
            firebrandHeightFromBurningPile_ = 0.0;
            flatDistanceFromBurningPile_ = 0.0;
            mountainDistanceFromBurningPile_ = 0.0;

            // Determine maximum firebrand height
            if ((windSpeedAtTwentyFeet > 1e-7) && (burningPileflameHeight > 1e-7))
            {
                // Determine maximum firebrand height
                firebrandHeightFromBurningPile_ = 12.2 * burningPileflameHeight;

                // Cover height used in calculation of flatDist.
                coverHeightUsedForBurningPile_ = calculateSpotCriticalCoverHeight(firebrandHeightFromBurningPile_, downwindCoverHeight);
                if (coverHeightUsedForBurningPile_ > 1e-7)
                {
                    // Flat terrain spotting distance.
                    flatDistanceFromBurningPile_ = 0.000718 * windSpeedAtTwentyFeet * sqrt(coverHeightUsedForBurningPile_)
                        * (0.362 + sqrt(firebrandHeightFromBurningPile_ / coverHeightUsedForBurningPile_) / 2.0
                            * log(firebrandHeightFromBurningPile_ / coverHeightUsedForBurningPile_));
                    // Adjust for mountainous terrain.
                    mountainDistanceFromBurningPile_ = spotDistanceMountainTerrain(flatDistanceFromBurningPile_,
                        location, ridgeToValleyDistance, ridgeToValleyElevation);
                    // Convert distances from miles to feet (base distance unit)
                    flatDistanceFromBurningPile_ = LengthUnits.toBaseUnits(flatDistanceFromBurningPile_, LengthUnits.LengthUnitsEnum.Miles);
                    mountainDistanceFromBurningPile_ = LengthUnits.toBaseUnits(mountainDistanceFromBurningPile_, LengthUnits.LengthUnitsEnum.Miles);
                }
            }
        }

        void calculateSpottingDistanceFromSurfaceFire()
        {
            // Get needed inputs
            SpotFireLocation.SpotFireLocationEnum location = spotInputs_.getLocation();
            double ridgeToValleyDistance = spotInputs_.getRidgeToValleyDistance(LengthUnits.LengthUnitsEnum.Miles);
            double ridgeToValleyElevation = spotInputs_.getRidgeToValleyElevation(LengthUnits.LengthUnitsEnum.Feet);
            double downwindCoverHeight = spotInputs_.getDownwindCoverHeight(LengthUnits.LengthUnitsEnum.Feet);
            double windSpeedAtTwentyFeet = spotInputs_.getWindSpeedAtTwentyFeet(SpeedUnits.SpeedUnitsEnum.MilesPerHour);
            double flameLength = spotInputs_.getSurfaceFlameLength(LengthUnits.LengthUnitsEnum.Feet);

            // Initialize return values
            firebrandHeightFromSurfaceFire_ = 0.0;
            flatDistanceFromSurfaceFire_ = 0.0;
            firebrandDrift_ = 0.0;

            // Determine maximum firebrand height
            if ((windSpeedAtTwentyFeet) > 1e-7 && (flameLength > 1e-7))
            {
                // f is a function relating thermal energy to windspeed.
                double f = 322.0 * pow((0.474 * windSpeedAtTwentyFeet), -1.01);

                // Byram's fireline intensity is derived back from flame length.
                double byrams = pow((flameLength / 0.45), (1.0 / 0.46));

                // Initial firebrand height (ft).
                firebrandHeightFromSurfaceFire_ = ((f * byrams) < 1e-7)
                    ? (0.0)
                    : (1.055 * sqrt(f * byrams));

                // Cover height used in calculation of localflatDistance.
                coverHeightUsedForSurfaceFire_ = calculateSpotCriticalCoverHeight(firebrandHeightFromSurfaceFire_, downwindCoverHeight);

                if (coverHeightUsedForSurfaceFire_ > 1e-7)
                {
                    firebrandDrift_ = 0.000278 * windSpeedAtTwentyFeet * pow(firebrandHeightFromSurfaceFire_, 0.643);
                    flatDistanceFromSurfaceFire_ = spotDistanceFlatTerrain(firebrandHeightFromSurfaceFire_, coverHeightUsedForSurfaceFire_, windSpeedAtTwentyFeet) + firebrandDrift_;
                    mountainDistanceFromSurfaceFire_ = spotDistanceMountainTerrain(flatDistanceFromSurfaceFire_,
                        location, ridgeToValleyDistance, ridgeToValleyElevation);
                    // Convert distances from miles to feet (base distance unit)
                    flatDistanceFromSurfaceFire_ = LengthUnits.toBaseUnits(flatDistanceFromSurfaceFire_, LengthUnits.LengthUnitsEnum.Miles);
                    mountainDistanceFromSurfaceFire_ = LengthUnits.toBaseUnits(mountainDistanceFromSurfaceFire_, LengthUnits.LengthUnitsEnum.Miles);
                }
            }
        }

        void calculateSpottingDistanceFromTorchingTrees()
        {
            // Get needed inputs
            SpotFireLocation.SpotFireLocationEnum location = spotInputs_.getLocation();
            double ridgeToValleyDistance = spotInputs_.getRidgeToValleyDistance(LengthUnits.LengthUnitsEnum.Miles);
            double ridgeToValleyElevation = spotInputs_.getRidgeToValleyElevation(LengthUnits.LengthUnitsEnum.Feet);
            double downwindCoverHeight = spotInputs_.getDownwindCoverHeight(LengthUnits.LengthUnitsEnum.Feet);
            double windSpeedAtTwentyFeet = spotInputs_.getWindSpeedAtTwentyFeet(SpeedUnits.SpeedUnitsEnum.MilesPerHour);
            double torchingTrees = spotInputs_.getTorchingTrees();
            double DBH = spotInputs_.getDBH(LengthUnits.LengthUnitsEnum.Inches);
            double treeHeight = spotInputs_.getTreeHeight(LengthUnits.LengthUnitsEnum.Feet);
            SpotTreeSpecies.SpotTreeSpeciesEnum treeSpecies = spotInputs_.getTreeSpecies();

            // Initialize return variables
            flameRatio_ = 0.0;
            flameHeightForTorchingTrees_ = 0.0;
            flameDuration_ = 0.0;
            firebrandHeightFromTorchingTrees_ = 0.0;
            flatDistanceFromTorchingTrees_ = 0.0;
            mountainDistanceFromTorchingTrees_ = 0.0;

            // Determine maximum firebrand height
            if (windSpeedAtTwentyFeet > 1e-7 && DBH > 1e-7 && torchingTrees >= 1.0)
            {
                // Catch species errors.
                if (!((int)treeSpecies < 0 || (int)treeSpecies >= 14))
                {
                    // Steady flame height (ft).
                    flameHeightForTorchingTrees_ = speciesFlameHeightParameters_[(int)treeSpecies][0]
                        * pow(DBH, speciesFlameHeightParameters_[(int)treeSpecies][1])
                        * pow(torchingTrees, 0.4);

                    flameRatio_ = treeHeight / flameHeightForTorchingTrees_;
                    // Steady flame duration.
                    flameDuration_ = speciesFlameDurationParameters_[(int)treeSpecies][0]
                        * pow(DBH, speciesFlameDurationParameters_[(int)treeSpecies][1])
                        * pow(torchingTrees, -0.2);

                    int i;
                    if (flameRatio_ >= 1.0)
                    {
                        i = 0;
                    }
                    else if (flameRatio_ >= 0.5)
                    {
                        i = 1;
                    }
                    else if (flameDuration_ < 3.5)
                    {
                        i = 2;
                    }
                    else
                    {
                        i = 3;
                    }

                    // Initial firebrand height (ft).
                    firebrandHeightFromTorchingTrees_ = firebrandHeightFactors_[i][0] * pow(flameDuration_, firebrandHeightFactors_[i][1]) * flameHeightForTorchingTrees_ + treeHeight / 2.0;

                    // Cover ht used in calculation of flatDist.
                    coverHeightUsedForTorchingTrees_ = calculateSpotCriticalCoverHeight(firebrandHeightFromTorchingTrees_, downwindCoverHeight);
                    if (coverHeightUsedForTorchingTrees_ > 1e-7)
                    {
                        flatDistanceFromTorchingTrees_ = spotDistanceFlatTerrain(firebrandHeightFromTorchingTrees_, coverHeightUsedForTorchingTrees_, windSpeedAtTwentyFeet);
                        mountainDistanceFromTorchingTrees_ = spotDistanceMountainTerrain(flatDistanceFromTorchingTrees_, location, ridgeToValleyDistance,
                            ridgeToValleyElevation);
                        // Convert distances from miles to feet (base distance unit)
                        flatDistanceFromTorchingTrees_ = LengthUnits.toBaseUnits(flatDistanceFromTorchingTrees_, LengthUnits.LengthUnitsEnum.Miles);
                        mountainDistanceFromTorchingTrees_ = LengthUnits.toBaseUnits(mountainDistanceFromTorchingTrees_, LengthUnits.LengthUnitsEnum.Miles);
                    }
                }
            }
        }

        void setBurningPileFlameHeight(double buringPileFlameHeight, LengthUnits.LengthUnitsEnum flameHeightUnits)
        {
            spotInputs_.setBurningPileFlameHeight(buringPileFlameHeight, flameHeightUnits);
        }

        void setDBH(double DBH, LengthUnits.LengthUnitsEnum DBHUnits)
        {
            spotInputs_.setDBH(DBH, DBHUnits);
        }

        void setDownwindCoverHeight(double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits)
        {
            spotInputs_.setDownwindCoverHeight(downwindCoverHeight, coverHeightUnits);
        }

        void setFlameLength(double flameLength, LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            spotInputs_.setSurfaceFlameLength(flameLength, flameLengthUnits);
        }

        void setLocation(SpotFireLocation.SpotFireLocationEnum location)
        {
            spotInputs_.setLocation(location);
        }

        void setRidgeToValleyDistance(double ridgeToValleyDistance, LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits)
        {
            spotInputs_.setRidgeToValleyDistance(ridgeToValleyDistance, ridgeToValleyDistanceUnits);
        }

        void setRidgeToValleyElevation(double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits)
        {
            spotInputs_.setRidgeToValleyElevation(ridgeToValleyElevation, elevationUnits);
        }

        void setTorchingTrees(int torchingTrees)
        {
            spotInputs_.setTorchingTrees(torchingTrees);
        }

        void setTreeHeight(double treeHeight, LengthUnits.LengthUnitsEnum treeHeightUnits)
        {
            spotInputs_.setTreeHeight(treeHeight, treeHeightUnits);
        }

        void setTreeSpecies(SpotTreeSpecies.SpotTreeSpeciesEnum treeSpecies)
        {
            spotInputs_.setTreeSpecies(treeSpecies);
        }

        void setWindSpeedAtTwentyFeet(double windSpeedAtTwentyFeet, SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            spotInputs_.setWindSpeedAtTwentyFeet(windSpeedAtTwentyFeet, windSpeedUnits);
        }

        void updateSpotInputsForBurningPile(SpotFireLocation.SpotFireLocationEnum location, double ridgeToValleyDistance,
            LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits, double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits,
            double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits, double buringPileFlameHeight,
            LengthUnits.LengthUnitsEnum flameHeightUnits, double windSpeedAtTwentyFeet, SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            spotInputs_.updateSpotInputsForBurningPile(location, ridgeToValleyDistance, ridgeToValleyDistanceUnits, ridgeToValleyElevation,
                elevationUnits, downwindCoverHeight, coverHeightUnits, buringPileFlameHeight, flameHeightUnits, windSpeedAtTwentyFeet,
                windSpeedUnits);
        }

        void updateSpotInputsForSurfaceFire(SpotFireLocation.SpotFireLocationEnum location, double ridgeToValleyDistance,
            LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits, double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits,
            double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits, double windSpeedAtTwentyFeet,
            SpeedUnits.SpeedUnitsEnum windSpeedUnits, double surfaceFlameLength, LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            spotInputs_.updateSpotInputsForSurfaceFire(location, ridgeToValleyDistance, ridgeToValleyDistanceUnits, ridgeToValleyElevation,
                elevationUnits, downwindCoverHeight, coverHeightUnits, windSpeedAtTwentyFeet, windSpeedUnits, surfaceFlameLength, flameLengthUnits);
        }

        void updateSpotInputsForTorchingTrees(SpotFireLocation.SpotFireLocationEnum location, double ridgeToValleyDistance,
            LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits, double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits,
            double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits, int torchingTrees, double DBH,
            LengthUnits.LengthUnitsEnum DBHUnits, double treeHeight, LengthUnits.LengthUnitsEnum treeHeightUnits,
            SpotTreeSpecies.SpotTreeSpeciesEnum treeSpecies, double windSpeedAtTwentyFeet, SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            spotInputs_.updateSpotInputsForTorchingTrees(location, ridgeToValleyDistance, ridgeToValleyDistanceUnits, ridgeToValleyElevation,
                elevationUnits, downwindCoverHeight, coverHeightUnits, torchingTrees, DBH, DBHUnits, treeHeight, treeHeightUnits, treeSpecies,
                windSpeedAtTwentyFeet, windSpeedUnits);
        }

        double getBurningPileFlameHeight(LengthUnits.LengthUnitsEnum flameHeightUnits)
        {
            return spotInputs_.getBurningPileFlameHeight(flameHeightUnits);
        }

        double getDBH(LengthUnits.LengthUnitsEnum DBHUnits)
        {
            return spotInputs_.getDBH(DBHUnits);
        }

        double getDownwindCoverHeight(LengthUnits.LengthUnitsEnum coverHeightUnits)
        {
            return spotInputs_.getDownwindCoverHeight(coverHeightUnits);
        }

        double getSurfaceFlameLength(LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            return spotInputs_.getSurfaceFlameLength(flameLengthUnits);
        }

        SpotFireLocation.SpotFireLocationEnum getLocation()
        {
            return spotInputs_.getLocation();
        }

        double getRidgeToValleyDistance(LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits)
        {
            return spotInputs_.getRidgeToValleyDistance(ridgeToValleyDistanceUnits);
        }

        double getRidgeToValleyElevation(LengthUnits.LengthUnitsEnum elevationUnits)
        {
            return spotInputs_.getRidgeToValleyElevation(elevationUnits);
        }

        int getTorchingTrees()
        {
            return spotInputs_.getTorchingTrees();
        }

        double getTreeHeight(LengthUnits.LengthUnitsEnum treeHeightUnits)
        {
            return spotInputs_.getTreeHeight(treeHeightUnits);
        }

        SpotTreeSpecies.SpotTreeSpeciesEnum getTreeSpecies()
        {
            return spotInputs_.getTreeSpecies();
        }

        double getWindSpeedAtTwentyFeet(SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            return spotInputs_.getWindSpeedAtTwentyFeet(windSpeedUnits);
        }

        double getCoverHeightUsedForBurningPile(LengthUnits.LengthUnitsEnum coverHeightUnits)
        {
            return LengthUnits.fromBaseUnits(coverHeightUsedForBurningPile_, coverHeightUnits);
        }

        double getCoverHeightUsedForSurfaceFire(LengthUnits.LengthUnitsEnum coverHeightUnits)
        {
            return LengthUnits.fromBaseUnits(coverHeightUsedForSurfaceFire_, coverHeightUnits);
        }

        double getCoverHeightUsedForTorchingTrees(LengthUnits.LengthUnitsEnum coverHeightUnits)
        {
            return LengthUnits.fromBaseUnits(coverHeightUsedForTorchingTrees_, coverHeightUnits);
        }

        double getFlameHeightForTorchingTrees(LengthUnits.LengthUnitsEnum flameHeightUnits)
        {
            return LengthUnits.fromBaseUnits(flameHeightForTorchingTrees_, flameHeightUnits);
        }

        double getFlameRatioForTorchingTrees()
        {
            return flameRatio_;
        }

        double getFlameDurationForTorchingTrees(TimeUnits.TimeUnitsEnum durationUnits)
        {
            return TimeUnits.fromBaseUnits(flameDuration_, durationUnits);
        }

        double getMaxFirebrandHeightFromBurningPile(LengthUnits.LengthUnitsEnum firebrandHeightUnits)
        {
            return LengthUnits.fromBaseUnits(firebrandHeightFromBurningPile_, firebrandHeightUnits);
        }

        double getMaxFirebrandHeightFromSurfaceFire(LengthUnits.LengthUnitsEnum firebrandHeightUnits)
        {
            return LengthUnits.fromBaseUnits(firebrandHeightFromSurfaceFire_, firebrandHeightUnits);
        }

        double getMaxFirebrandHeightFromTorchingTrees(LengthUnits.LengthUnitsEnum firebrandHeightUnits)
        {
            return LengthUnits.fromBaseUnits(firebrandHeightFromTorchingTrees_, firebrandHeightUnits);
        }

        double getMaxFlatTerrainSpottingDistanceFromBurningPile(LengthUnits.LengthUnitsEnum spottingDistanceUnits)
        {
            return LengthUnits.fromBaseUnits(flatDistanceFromBurningPile_, spottingDistanceUnits);
        }

        double getMaxFlatTerrainSpottingDistanceFromSurfaceFire(LengthUnits.LengthUnitsEnum spottingDistanceUnits)
        {
            return LengthUnits.fromBaseUnits(flatDistanceFromSurfaceFire_, spottingDistanceUnits);
        }

        double getMaxFlatTerrainSpottingDistanceFromTorchingTrees(LengthUnits.LengthUnitsEnum spottingDistanceUnits)
        {
            return LengthUnits.fromBaseUnits(flatDistanceFromTorchingTrees_, spottingDistanceUnits);
        }

        double getMaxMountainousTerrainSpottingDistanceFromBurningPile(LengthUnits.LengthUnitsEnum spottingDistanceUnits)
        {
            return LengthUnits.fromBaseUnits(mountainDistanceFromBurningPile_, spottingDistanceUnits);
        }

        double getMaxMountainousTerrainSpottingDistanceFromSurfaceFire(LengthUnits.LengthUnitsEnum spottingDistanceUnits)
        {
            return LengthUnits.fromBaseUnits(mountainDistanceFromSurfaceFire_, spottingDistanceUnits);
        }

        double getMaxMountainousTerrainSpottingDistanceFromTorchingTrees(LengthUnits.LengthUnitsEnum spottingDistanceUnits)
        {
            return LengthUnits.fromBaseUnits(mountainDistanceFromTorchingTrees_, spottingDistanceUnits);
        }
    }
}