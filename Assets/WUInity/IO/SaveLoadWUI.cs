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
            string json = JsonUtility.ToJson(WUInity.WUINITY_IN, true);
            System.IO.File.WriteAllText(WUInity.WORKING_FILE, json); //Application.dataPath + "/Resources/_input/" + filename + ".wui"
            EvacGroup.SaveEvacGroupIndices();
            GraphicalFireInput.SaveGraphicalFireInput();
        }

        public static void LoadInput(string path)
        {
            string input = System.IO.File.ReadAllText(path);
            if(input != null)
            {
                WUInityInput wui = JsonUtility.FromJson<WUInityInput>(input);
                WUInity.WORKING_FILE = path;
                WUInity.WUINITY.LoadInputData(wui);
            }
            else
            {
                WUInity.WUINITY_SIM.LogMessage("WARNING: Input file not found.");
            }
        }

        public static void LoadDefaultInputs()
        {
            string path = Path.Combine(WUInity.DATA_FOLDER, "default/default.wui");
            LoadInput(path);         
        }

        public static void SaveOutput(string filename)
        {
            string json = JsonUtility.ToJson(WUInity.WUINITY_OUT, true);
            string path = Path.Combine(WUInity.WORKING_FOLDER, filename + ".wuiout");
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Saves a collection of routes by converting to a format that is serializable (Itinero objects does not serialize using Unitys JSONUtility)
        /// </summary>
        /// <param name="filename"></param>
        public static void SaveRouteCollections(string filename)
        {    
            string path = Path.Combine(WUInity.WORKING_FOLDER, filename + ".rc");
            
            RouteCollectionWrapper save = new  RouteCollectionWrapper(WUInity.WUINITY_SIM.GetRouteCollection());
            string json = JsonUtility.ToJson(save, false);
            System.IO.File.WriteAllText(path, json);

            //slow
            /*BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Create(path);
            bf.Serialize(file, save);
            file.Close();*/

            WUInity.WUINITY_SIM.LogMessage("Saved route collection to " + path);
        }

        /// <summary>
        /// Loads wrapper format and converts to a proper route collection containing Itinero.Routes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static RouteCollection[] LoadRouteCollections(string filename)
        {
            string path = Path.Combine(WUInity.WORKING_FOLDER, filename + ".rc");
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

                WUInity.WUINITY_SIM.LogMessage("Loaded route collection from " + path);
                return collection;
            }
            else
            {
                WUInity.WUINITY_SIM.LogMessage("WARNING: Route collection file not found in " + path);
            }

            return null;
        }
    }
}

