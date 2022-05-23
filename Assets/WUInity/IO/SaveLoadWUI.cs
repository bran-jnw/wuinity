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

            WUInity.WUI_LOG("LOG: Input file " + WUInity.WORKING_FILE + " saved.");
        }

        public static void LoadInput(string path)
        {
            string input = File.ReadAllText(path);
            if(input != null)
            {
                WUInityInput wui = JsonUtility.FromJson<WUInityInput>(input);
                WUInity.WORKING_FILE = path;
                WUInity.INSTANCE.SetNewInputData(wui);
                WUInity.WUI_LOG("LOG: Input file " + WUInity.WORKING_FILE + " loaded.");
            }
            else
            {
                WUInity.WUI_LOG("ERROR: Input file not found.");
            }
        }

        public static void SaveOutput(string filename)
        {
            List<string> log = WUInity.GetLog();
            string path = Path.Combine(WUInity.WORKING_FOLDER, filename + ".wuiout");
            //System.IO.File.WriteAllText(path, log);
        }

        /// <summary>
        /// Saves a collection of routes by converting to a format that is serializable (Itinero objects does not serialize using Unitys JSONUtility)
        /// </summary>
        /// <param name="filename"></param>
        public static void SaveRouteCollections()
        {    
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simName + ".rc");
            
            RouteCollectionWrapper save = new  RouteCollectionWrapper(WUInity.SIM_DATA.GetRouteCollection());
            string json = JsonUtility.ToJson(save, false);
            System.IO.File.WriteAllText(path, json);

            //slow
            /*BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Create(path);
            bf.Serialize(file, save);
            file.Close();*/

            WUInity.WUI_LOG("LOG: Saved route collection to " + path);
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

                if(collection == null)
                {
                    WUInity.WUI_LOG("ERROR: Tried loading route collection from " + path + " but route collection is not valid for current input.");
                }
                else
                {
                    WUInity.WUI_LOG("LOG: Loaded route collection from " + path);
                }
                
                return collection;
            }
            else
            {
                WUInity.WUI_LOG("WARNING: Route collection file not found in " + path + ", have to be built at runtime (will take some time).");
                return null;
            }            
        }
    }
}

