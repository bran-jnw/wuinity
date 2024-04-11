using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.Runtime
{
    public class TrafficData
    {

        public void LoadAll()
        {
            LoadRoadTypeData(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Traffic.roadTypesFile), false);
            LoadOpticalDensityFile(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Traffic.opticalDensityFile), false);            
        }

        Traffic.OpticalDensityRamp _opticalDensity;
        public Traffic.OpticalDensityRamp OpticalDensity
        {
            get
            {
                return _opticalDensity;
            }            
        }

        private Traffic.RoadTypeData _roadTypeData;
        public Traffic.RoadTypeData RoadTypeData
        {
            get
            {
                return _roadTypeData;
            }
        }

        public bool LoadRoadTypeData(string path, bool updateInputFile)
        {
            //success in this case means that we loaded a file and not defaults
            bool success;
            _roadTypeData = Traffic.RoadTypeData.LoadRoadTypeData(path, out success);
            if(success && updateInputFile)
            {
                WUIEngine.INPUT.Traffic.roadTypesFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadOpticalDensityFile(string path, bool updateInputFile)
        {
            bool success = false;

            _opticalDensity = new Traffic.OpticalDensityRamp();
            success = _opticalDensity.LoadOpticalDensityRampFile(path);
            WUIEngine.DATA_STATUS.OpticalDensityLoaded = success;
            if(success && updateInputFile)
            {
                WUIEngine.INPUT.Traffic.opticalDensityFile = Path.GetFileName(path); ;
                WUIEngineInput.SaveInput();
            }

            return success;
        }
    }
}