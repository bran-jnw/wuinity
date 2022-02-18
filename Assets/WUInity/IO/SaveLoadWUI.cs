using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;


namespace WUInity
{
    public class SaveLoadWUI
    {
        public static void SaveInput()
        {            
            string json = JsonUtility.ToJson(WUInity.INPUT, true);
            System.IO.File.WriteAllText(WUInity.WORKING_FILE, json);
            EvacGroup.SaveEvacGroupIndices();
            GraphicalFireInput.SaveGraphicalFireInput();

            WUInity.SIM.LogMessage("LOG: Input file " + WUInity.WORKING_FILE + " saved.");
        }

        public static void LoadInput(string path)
        {
            string input = System.IO.File.ReadAllText(path);
            if(input != null)
            {
                WUInityInput wui = JsonUtility.FromJson<WUInityInput>(input);
                WUInity.WORKING_FILE = path;
                WUInity.INSTANCE.LoadInputData(wui);
                WUInity.SIM.LogMessage("LOG: Input file " + WUInity.WORKING_FILE + " loaded.");
            }
            else
            {
                WUInity.SIM.LogMessage("WARNING: Input file not found.");
            }
        }

        public static void LoadDefaultInputs()
        {
            string path = Path.Combine(WUInity.DATA_FOLDER, "default/default.wui");
            LoadInput(path);         
        }

        public static void SaveOutput(string filename)
        {
            string json = JsonUtility.ToJson(WUInity.OUTPUT, true);
            string path = Path.Combine(WUInity.WORKING_FOLDER, filename + ".wuiout");
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Saves a collection of routes by converting to a format that is serializable (Itinero objects does not serialize using Unitys JSONUtility)
        /// </summary>
        /// <param name="filename"></param>
        public static void SaveRouteCollections()
        {    
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simName + ".rc");
            
            RouteCollectionWrapper save = new  RouteCollectionWrapper(WUInity.SIM.GetRouteCollection());
            string json = JsonUtility.ToJson(save, false);
            System.IO.File.WriteAllText(path, json);

            //slow
            /*BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Create(path);
            bf.Serialize(file, save);
            file.Close();*/

            WUInity.SIM.LogMessage("Saved route collection to " + path);
        }

        /// <summary>
        /// Loads wrapper format and converts to a proper route collection containing Itinero.Routes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static RouteCollection[] LoadRouteCollections()
        {
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simName + ".rc");
            if(System.IO.File.Exists(path))
            {
                string input = System.IO.File.ReadAllText(path);
                RouteCollectionWrapper rCWS = JsonUtility.FromJson<RouteCollectionWrapper>(input);

                //slow
                /*BinaryFormatter bf = new BinaryFormatter();
                System.IO.FileStream file = System.IO.File.Open(path, System.IO.FileMode.Open);
                RouteCollectionWrapper rCWS = (RouteCollectionWrapper)bf.Deserialize(file);
                file.Close();*/

                RouteCollection[] collection = rCWS.Convert();
                rCWS = null;
                System.GC.Collect();

                WUInity.SIM.LogMessage("Loaded route collection from " + path);
                return collection;
            }
            else
            {
                WUInity.SIM.LogMessage("WARNING: Route collection file not found in " + path);
            }

            return null;
        }
    }
}

