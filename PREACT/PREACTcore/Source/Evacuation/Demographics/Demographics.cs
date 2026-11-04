using System.Collections.Generic;
using System.IO;

namespace PREACT.Evacuation
{
    public class Demographics
    {
        private string _name;
        private HouseholdDemographics _householdDemographics;
        private VehicleDemographics _vehicleDemographics;
        private bool _default;

        public string Name { get => _name; }
        public HouseholdDemographics HouseholdDemographics { get => _householdDemographics; }
        public VehicleDemographics VehicleDemographics { get => _vehicleDemographics; }
        public bool Default { get => _default; }

        public Demographics(HouseholdDemographics householdDemographics, VehicleDemographics vehicleDemographics, DemographicsInput demographicsInput)
        {
            _householdDemographics = householdDemographics;
            _vehicleDemographics = vehicleDemographics;
            _default = demographicsInput.Default;
        }

        public static Dictionary<string, Demographics> CreateDemographicsFromInput(Dictionary<string, DemographicsInput> demographicsInput, Dictionary<string,HouseholdDemographics> allHouseholdDemographics, Dictionary<string, VehicleDemographics> allVehicleDemographics)
        {
            Dictionary<string, Demographics> demographics = new Dictionary<string, Demographics>(demographicsInput.Count);

            foreach (KeyValuePair<string, DemographicsInput> d in demographicsInput)
            {
                HouseholdDemographics? hResult;
                VehicleDemographics? vResult;
                

                if (allHouseholdDemographics.TryGetValue(d.Value.HouseholdDemographics, out hResult) && allVehicleDemographics.TryGetValue(d.Value.VehicleDemographics, out vResult))
                {
                    demographics.Add(d.Value.Name, new Demographics(hResult, vResult, d.Value));
                }
            }

            return demographics;
        }

    }
}
