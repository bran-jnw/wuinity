using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvacuationPopulationSimulator
{
    public enum HouseholdType { Single, Couple, NuclearFamily, MultiGenerational }

    public class Person
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public bool IsMobilityImpaired { get; set; } // Crucial for evacuation routing

        public Role OccupationalRole { get; set; }

        // Sleep parameters
        public double Bedtime { get; set; }   // Hours past midnight (e.g., 22.5)
        public double WakeTime { get; set; }  // Hours past midnight (e.g., 6.5)

        // Daytime locations (represented as node/coordinate IDs in your spatial graph)
        public int WorkSchoolLocationId { get; set; }
    }

    public class Household
    {
        public int Id { get; set; }
        public HouseholdType Type { get; set; }
        public List<Person> Members { get; set; } = new List<Person>();
        public int VehicleCount { get; set; } // Evacuation asset tracking
        public int HomeLocationId { get; set; }

        // Evacuation performance metrics derived from internal dependencies
        public int GetSlowestMemberAge()
        {
            int maxAge = 0;
            foreach (var m in Members) if (m.Age > maxAge) maxAge = m.Age;
            return maxAge;
        }

        public bool HasDependent()
        {
            return Members.Exists(m => m.Age < 10 || m.Age > 75 || m.IsMobilityImpaired);
        }

        // Evacuation Evaluation Engine
        public (double ReactionDelayMinutes, bool RequiresRecombination) EvaluateEvacuationState(double disasterTime)
        {
            bool isNight = disasterTime < 6.0 || disasterTime > 23.0;
            bool isSchoolWorkHours = disasterTime >= 8.5 && disasterTime <= 15.0;

            // 1. Determine baseline cognitive reaction delay based on sleep states
            double baselineDelay = 5.0; // 5 minutes baseline to check phone/pack bags

            if (isNight)
            {
                foreach (var member in Members)
                {
                    if (IsAsleep(member, disasterTime))
                    {
                        // Sleep inertia + waking up household dramatically increases reaction time
                        baselineDelay = 25.0;
                        break;
                    }
                }
            }

            // 2. Check for Daycare/School Recombination requirements
            bool requiresRecombination = false;
            if (isSchoolWorkHours)
            {
                bool hasParentsAtWork = Members.Exists(m => m.OccupationalRole == Role.Worker);
                bool hasKidsAtSchool = Members.Exists(m => m.OccupationalRole == Role.Student || m.OccupationalRole == Role.Toddler);

                // If parents are away and kids are at school, they MUST recombine first
                if (hasParentsAtWork && hasKidsAtSchool)
                {
                    requiresRecombination = true;
                }
            }

            return (baselineDelay, requiresRecombination);
        }

        private bool IsAsleep(Person p, double hour)
        {
            if (p.Bedtime > p.WakeTime) // Sleeps past midnight
                return hour >= p.Bedtime || hour < p.WakeTime;
            else // Off-beat schedule
                return hour >= p.Bedtime && hour < p.WakeTime;
        }
    }

    class PopulationGenerator
    {
        private static readonly Random _rand = new Random();
        private static int _personIdCounter = 0;

        static void Main(string[] args)
        {
            int targetHouseholds = 250000; // Simulating a mid-sized city region
            Console.WriteLine($"Synthesizing {targetHouseholds:N0} structured households...");

            List<Household> regionHouseholds = GenerateRegionPopulation(targetHouseholds);

            // Metrics analysis to verify dependencies
            int totalPopulation = 0;
            int totalVehicles = 0;
            int householdsWithDependents = 0;

            foreach (var hh in regionHouseholds)
            {
                totalPopulation += hh.Members.Count;
                totalVehicles += hh.VehicleCount;
                if (hh.HasDependent()) householdsWithDependents++;
            }

            Console.WriteLine("\n--- Synthetic Population Structural Diagnostics ---");
            Console.WriteLine($"Total Generated Citizens: {totalPopulation:N0}");
            Console.WriteLine($"Average Household Size:  {(double)totalPopulation / targetHouseholds:F2} persons");
            Console.WriteLine($"Total Evacuation Assets: {totalVehicles:N0} vehicles");
            Console.WriteLine($"HNs Gated by Dependents: {((double)householdsWithDependents / targetHouseholds) * 100:F1}% (Will delay evacuation)");
        }

        public static List<Household> GenerateRegionPopulation(int count)
        {
            List<Household> households = new List<Household>(count);

            for (int i = 0; i < count; i++)
            {
                Household hh = new Household { Id = i };

                // Dependency 1: Size Distribution determines the structural Family Template Type
                double sizeRoll = _rand.NextDouble();
                int size;
                if (sizeRoll < 0.28) { size = 1; hh.Type = HouseholdType.Single; }
                else if (sizeRoll < 0.63) { size = 2; hh.Type = HouseholdType.Couple; } // Or single parent + 1 child
                else if (sizeRoll < 0.80) { size = 3; hh.Type = HouseholdType.NuclearFamily; }
                else if (sizeRoll < 0.95) { size = 4; hh.Type = HouseholdType.NuclearFamily; }
                else { size = _rand.Next(5, 7); hh.Type = HouseholdType.MultiGenerational; }

                // Dependency 2: Generate the Reference Person (Head of Household) age based on a standard curve
                int referenceAge = GenerateAgeFromBellCurve(38, 14, 18, 85);

                // Dependency 3: Sub-sample other members strictly conditioned on the Reference Person's Age
                switch (hh.Type)
                {
                    case HouseholdType.Single:
                        hh.Members.Add(CreatePerson(referenceAge));
                        break;

                    case HouseholdType.Couple:
                        hh.Members.Add(CreatePerson(referenceAge));
                        // Spouses track tightly in age (Correlation dependency ~ ±4 years variance)
                        int spouseAge = GenerateAgeFromBellCurve(referenceAge, 3, 18, 85);
                        hh.Members.Add(CreatePerson(spouseAge));
                        break;

                    case HouseholdType.NuclearFamily:
                        hh.Members.Add(CreatePerson(referenceAge));
                        int partnerAge = GenerateAgeFromBellCurve(referenceAge, 3, 18, 85);
                        hh.Members.Add(CreatePerson(partnerAge));

                        // Children's ages depend entirely on parent's age (Assuming parenting window between 20 and 42)
                        int childCount = size - 2;
                        for (int c = 0; c < childCount; c++)
                        {
                            int maxChildAge = Math.Max(0, referenceAge - 19);
                            int minChildAge = Math.Max(0, referenceAge - 42);
                            int childAge = _rand.Next(minChildAge, Math.Min(maxChildAge, 18));
                            hh.Members.Add(CreatePerson(childAge));
                        }
                        break;

                    case HouseholdType.MultiGenerational:
                        // High density dependency: Grandparent, parents, and children present
                        hh.Members.Add(CreatePerson(referenceAge)); // Middle generation parent
                        int spouseAgeMG = GenerateAgeFromBellCurve(referenceAge, 3, 18, 85);
                        hh.Members.Add(CreatePerson(spouseAgeMG));

                        // Add a grandparent (Generation +1 dependency)
                        int grandparentAge = GenerateAgeFromBellCurve(referenceAge + 25, 4, 60, 95);
                        hh.Members.Add(CreatePerson(grandparentAge));

                        // Remaining slots allocated to children
                        while (hh.Members.Count < size)
                        {
                            int childAge = _rand.Next(0, Math.Min(referenceAge - 19, 22));
                            hh.Members.Add(CreatePerson(childAge));
                        }
                        break;
                }

                // Dependency 4: Vehicle ownership scales with household size and geographic wealth markers
                double vehRoll = _rand.NextDouble();
                if (hh.Type == HouseholdType.Single)
                {
                    hh.VehicleCount = vehRoll < 0.20 ? 0 : 1; // 20% of single households have no car
                }
                else
                {
                    hh.VehicleCount = vehRoll < 0.05 ? 0 : (vehRoll < 0.45 ? 1 : 2);
                }

                households.Add(hh);
            }

            return households;
        }

        private static Person CreatePerson(int age)
        {
            // Internal constraint mapping: Mobility impairments scale exponentially with age bracket
            bool impaired = false;
            if (age > 65) impaired = _rand.NextDouble() < 0.12; // 12% baseline for seniors
            if (age > 80) impaired = _rand.NextDouble() < 0.35; // 35% for elderly outliers

            return new Person
            {
                Id = System.Threading.Interlocked.Increment(ref _personIdCounter),
                Age = age,
                IsMobilityImpaired = impaired
            };
        }

        // Box-Muller configuration helper to isolate distribution trends within demographic constraints
        private static int GenerateAgeFromBellCurve(double mean, double stdDev, int min, int max)
        {
            double u1 = 1.0 - _rand.NextDouble();
            double u2 = 1.0 - _rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double val = mean + stdDev * randStdNormal;
            return (int)Math.Clamp(val, min, max);
        }
    }

    //next 
    public enum Role { Toddler, Student, Worker, StayAtHome, Retired }
    public enum HouseholdState { CoLocatedSleep, CoLocatedAwake, SeparatedDaytime }


    class Program
    {
        private static readonly Random _rand = new Random();

        static void Main(string[] args)
        {
            // Create a sample household: Parents (38, 36) and two kids (10, 4)
            Household hh = CreateSampleFamily();

            // Evaluate how this family responds to disasters at different times
            double[] disasterScenarios = { 2.0, 11.5, 18.0 }; // 2 AM, 11:30 AM, 6 PM

            Console.WriteLine($"Analyzing Evacuation Responses for Household {hh.Id} (4 members):");
            foreach (double hour in disasterScenarios)
            {
                var (delay, recombine) = hh.EvaluateEvacuationState(hour);
                string locationState = hour == 11.5 ? "SEPARATED (Work/School)" : "CO-LOCATED (Home)";

                Console.WriteLine($"\n[Disaster Time: {FormatTime(hour)}] - Status: {locationState}");
                Console.WriteLine($" -> Household Mobilization Delay: {delay} minutes");
                Console.WriteLine($" -> Requires School/Daycare Pickup Phase: {(recombine ? "YES (High local traffic impact)" : "NO")}");
            }
        }

        private static Household CreateSampleFamily()
        {
            Household hh = new Household { Id = 101, HomeLocationId = 9001, VehicleCount = 2 };

            // Dad - Worker
            hh.Members.Add(new Person
            {
                Id = 1,
                Age = 38,
                OccupationalRole = Role.Worker,
                Bedtime = 23.0,
                WakeTime = 6.5,
                WorkSchoolLocationId = 505
            });
            // Mom - Worker
            hh.Members.Add(new Person
            {
                Id = 2,
                Age = 36,
                OccupationalRole = Role.Worker,
                Bedtime = 22.5,
                WakeTime = 6.0,
                WorkSchoolLocationId = 506
            });
            // Older Kid - Student
            hh.Members.Add(new Person
            {
                Id = 3,
                Age = 10,
                OccupationalRole = Role.Student,
                Bedtime = 21.0,
                WakeTime = 7.0,
                WorkSchoolLocationId = 301 // Elementary school
            });
            // Youngest Kid - Toddler
            hh.Members.Add(new Person
            {
                Id = 4,
                Age = 4,
                OccupationalRole = Role.Toddler,
                Bedtime = 20.0,
                WakeTime = 7.0,
                WorkSchoolLocationId = 401 // Daycare
            });

            return hh;
        }

        private static string FormatTime(double hour)
        {
            int h = (int)Math.Floor(hour);
            int m = (int)((hour - h) * 60);
            return $"{h:D2}:{m:D2}";
        }
    }
}
