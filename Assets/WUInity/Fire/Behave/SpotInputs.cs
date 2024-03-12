using static WUInity.Fire.BehaveUnits;

namespace WUInity.Fire
{
    public struct SpotTreeSpecies
    {
        public enum SpotTreeSpeciesEnum
        {
            ENGELMANN_SPRUCE = 0,    //  0 Engelmann spruce (Picea engelmannii)
            DOUGLAS_FIR = 1,    //  1 Douglas-fir (Pseudotsuga menziessii)
            SUBALPINE_FIR = 2,    //  2 Subalpine fir (Abies lasiocarpa)
            WESTERN_HEMLOCK = 3,    //  3 Western hemlock (Tsuga heterophylla)
            PONDEROSA_PINE = 4,    //  4 Ponderosa pine (Pinus ponderosa)
            LODGEPOLE_PINE = 5,    //  5 Lodgepole pine (Pinus contora)
            WESTERN_WHITE_PINE = 6,    //  6 Western white pine (Pinus monticola)
            GRAND_FIR = 7,    //  7 Grand fir (Abies grandis)
            BALSAM_FIR = 8,    //  8 Balsam fir (Abies balsamea)
            SLASH_PINE = 9,    //  9 Slash pine (Pinus elliottii)
            LONGLEAF_PINE = 10,   // 10 Longleaf pine (Pinus palustrus)
            POND_PINE = 11,   // 11 Pond pine (Pinus serotina)
            SHORTLEAF_PINE = 12,   // 12 Shortleaf pine (Pinus echinata)
            LOBLOLLY_PINE = 13    // 13 Loblolly pine (Pinus taeda)
        };
    };

    public struct SpotFireLocation
    {
        public enum SpotFireLocationEnum
        {
            MIDSLOPE_WINDWARD = 0,    // midslope, windward
            VALLEY_BOTTOM = 1,    // valley bottom
            MIDSLOPE_LEEWARD = 2,    // midslope, leeward
            RIDGE_TOP = 3,    // ridge top
        };
    };

    public class SpotInputs
    {
        public struct SpotArrayConstants
        {
            public enum SpotArrayConstantsEnum
            {
                NUM_COLS = 2,
                NUM_FIREBRAND_ROWS = 4,
                NUM_SPECIES = 14
            };
        };

        double DBH_;
        double downwindCoverHeight_;
        SpotFireLocation.SpotFireLocationEnum location_;
        double ridgeToValleyDistance_;
        double ridgeToValleyElevation_;
        double windSpeedAtTwentyFeet_;
        double buringPileFlameHeight_;
        double surfaceFlameLength_;
        int torchingTrees_;
        double treeHeight_;
        SpotTreeSpecies.SpotTreeSpeciesEnum treeSpecies_;

        public SpotInputs()
        {
	        initializeMembers();
        }

        public void setBurningPileFlameHeight(double buringPileFlameHeight, LengthUnits.LengthUnitsEnum flameHeightUnits)
        {
            buringPileFlameHeight_ = LengthUnits.toBaseUnits(buringPileFlameHeight, flameHeightUnits);
        }

        public void setDBH(double DBH, LengthUnits.LengthUnitsEnum DBHUnits)
        {
            DBH_ = LengthUnits.toBaseUnits(DBH, DBHUnits);
        }

        public void setDownwindCoverHeight(double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits)
        {
            downwindCoverHeight_ = LengthUnits.toBaseUnits(downwindCoverHeight, coverHeightUnits);
        }

        public void setSurfaceFlameLength(double surfaceFlameLength, LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            surfaceFlameLength_ = LengthUnits.toBaseUnits(surfaceFlameLength, flameLengthUnits);
        }

        public void setLocation(SpotFireLocation.SpotFireLocationEnum location)
        {
            location_ = location;
        }

        public void setRidgeToValleyDistance(double ridgeToValleyDistance, LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits)
        {
            ridgeToValleyDistance_ = LengthUnits.toBaseUnits(ridgeToValleyDistance, ridgeToValleyDistanceUnits);
        }

        public void setRidgeToValleyElevation(double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits)
        {
            ridgeToValleyElevation_ = LengthUnits.toBaseUnits(ridgeToValleyElevation, elevationUnits);
        }

        public void setTorchingTrees(int torchingTrees)
        {
            torchingTrees_ = torchingTrees;
        }

        public void setTreeHeight(double treeHeight, LengthUnits.LengthUnitsEnum treeHeightUnits)
        {
            treeHeight_ = LengthUnits.toBaseUnits(treeHeight, treeHeightUnits);
        }

        public void setTreeSpecies(SpotTreeSpecies.SpotTreeSpeciesEnum treeSpecies)
        {
            treeSpecies_ = treeSpecies;
        }

        public void setWindSpeedAtTwentyFeet(double windSpeedAtTwentyFeet, SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            windSpeedAtTwentyFeet_ = SpeedUnits.toBaseUnits(windSpeedAtTwentyFeet, windSpeedUnits);
        }

        public void updateSpotInputsForBurningPile(SpotFireLocation.SpotFireLocationEnum location, double ridgeToValleyDistance,
            LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits, double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits,
            double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits, double buringPileFlameHeight,
            LengthUnits.LengthUnitsEnum flameHeightUnits, double windSpeedAtTwentyFeet, SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            setLocation(location);
            setRidgeToValleyDistance(ridgeToValleyDistance, ridgeToValleyDistanceUnits);
            setRidgeToValleyElevation(ridgeToValleyElevation, elevationUnits);
            setDownwindCoverHeight(downwindCoverHeight, coverHeightUnits);
            setWindSpeedAtTwentyFeet(windSpeedAtTwentyFeet, windSpeedUnits);
            setBurningPileFlameHeight(buringPileFlameHeight, flameHeightUnits);
        }

        public void updateSpotInputsForSurfaceFire(SpotFireLocation.SpotFireLocationEnum location, double ridgeToValleyDistance,
            LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits, double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits,
            double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits, double windSpeedAtTwentyFeet,
            SpeedUnits.SpeedUnitsEnum windSpeedUnits, double surfaceFlameLength, LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            setLocation(location);
            setRidgeToValleyDistance(ridgeToValleyDistance, ridgeToValleyDistanceUnits);
            setRidgeToValleyElevation(ridgeToValleyElevation, elevationUnits);
            setDownwindCoverHeight(downwindCoverHeight, coverHeightUnits);
            setWindSpeedAtTwentyFeet(windSpeedAtTwentyFeet, windSpeedUnits);
            setSurfaceFlameLength(surfaceFlameLength, flameLengthUnits);
        }

        public void updateSpotInputsForTorchingTrees(SpotFireLocation.SpotFireLocationEnum location, double ridgeToValleyDistance,
            LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits, double ridgeToValleyElevation, LengthUnits.LengthUnitsEnum elevationUnits,
            double downwindCoverHeight, LengthUnits.LengthUnitsEnum coverHeightUnits, int torchingTrees, double DBH,
            LengthUnits.LengthUnitsEnum DBHUnits, double treeHeight, LengthUnits.LengthUnitsEnum treeHeightUnits,
            SpotTreeSpecies.SpotTreeSpeciesEnum treeSpecies, double windSpeedAtTwentyFeet, SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            setLocation(location);
            setRidgeToValleyDistance(ridgeToValleyDistance, ridgeToValleyDistanceUnits);
            setRidgeToValleyElevation(ridgeToValleyElevation, elevationUnits);
            setDownwindCoverHeight(downwindCoverHeight, coverHeightUnits);
            setTorchingTrees(torchingTrees);
            setDBH(DBH, DBHUnits);
            setTreeHeight(treeHeight, treeHeightUnits);
            setTreeSpecies(treeSpecies);
            setWindSpeedAtTwentyFeet(windSpeedAtTwentyFeet, windSpeedUnits);
        }

        public double getBurningPileFlameHeight(LengthUnits.LengthUnitsEnum flameHeightUnits)
        {
            return LengthUnits.fromBaseUnits(buringPileFlameHeight_, flameHeightUnits);
        }

        public double getDBH(LengthUnits.LengthUnitsEnum DBHUnits)
        {
            return LengthUnits.fromBaseUnits(DBH_, DBHUnits);
        }

        public double getDownwindCoverHeight(LengthUnits.LengthUnitsEnum coverHeightUnits)
        {
            return LengthUnits.fromBaseUnits(downwindCoverHeight_, coverHeightUnits);
        }

        public double getSurfaceFlameLength(LengthUnits.LengthUnitsEnum surfaceFlameLengthUnits)
        {
            return LengthUnits.fromBaseUnits(surfaceFlameLength_, surfaceFlameLengthUnits);
        }

        public SpotFireLocation.SpotFireLocationEnum getLocation()
        {
            return location_;
        }

        public double getRidgeToValleyDistance(LengthUnits.LengthUnitsEnum ridgeToValleyDistanceUnits)
        {
            return LengthUnits.fromBaseUnits(ridgeToValleyDistance_, ridgeToValleyDistanceUnits);
        }

        public double getRidgeToValleyElevation(LengthUnits.LengthUnitsEnum elevationUnits)
        {
            return LengthUnits.fromBaseUnits(ridgeToValleyElevation_, elevationUnits);
        }

        public int getTorchingTrees()
        {
            return torchingTrees_;
        }

        public double getTreeHeight(LengthUnits.LengthUnitsEnum treeHeightUnits)
        {
            return LengthUnits.fromBaseUnits(treeHeight_, treeHeightUnits);
        }

        public SpotTreeSpecies.SpotTreeSpeciesEnum getTreeSpecies()
        {
            return treeSpecies_;
        }

        public double getWindSpeedAtTwentyFeet(SpeedUnits.SpeedUnitsEnum windSpeedUnits)
        {
            return SpeedUnits.fromBaseUnits(windSpeedAtTwentyFeet_, windSpeedUnits);
        }

        void initializeMembers()
        {
            downwindCoverHeight_ = 0.0;
            location_ = SpotFireLocation.SpotFireLocationEnum.MIDSLOPE_WINDWARD;
            ridgeToValleyDistance_ = 0.0;
            ridgeToValleyElevation_ = 0.0;
            windSpeedAtTwentyFeet_ = 0.0;
            buringPileFlameHeight_ = 0.0;
            surfaceFlameLength_ = 0.0;
            torchingTrees_ = 0;
            DBH_ = 0.0;
            treeHeight_ = 0.0;
        }
    }
}

