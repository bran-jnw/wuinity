using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace WUIPlatform.Fire
{
    [System.Serializable]
    public struct WindData                  
    {
        public float time;
        public float direction;
        public float speed;
        public float cloudCover;

        bool dataIsInSeconds;

        int month, day, hour;

        public WindData(float time, float direction, float speed, float cloudCover)    
        {
            this.time = time;
            this.direction = direction;
            this.speed = speed;
            this.cloudCover = cloudCover;

            dataIsInSeconds = true;

            month = 0;
            day = 0;
            hour = 0;
        }

        public WindData(int month, int day, int hour, float direction, float speed, float cloudCover) 
        {
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.direction = direction;
            this.speed = speed;
            this.cloudCover = cloudCover;

            dataIsInSeconds = false;
            
            time = 0.0f;
        }

        //TODO: implement
        public void ConvertToSeconds(float fireStartTime)
        {

        }
    }
    [System.Serializable]
    public class WindInput
    {
        private WindData[] dataPoints;         

        private CatmullRomSpline1D directionSpline;              
        private CatmullRomSpline1D speedSpline;
        private CatmullRomSpline1D cloudSpline;

        public WindInput(WindData[] dataPoints)                 
        {
            this.dataPoints = dataPoints;
        }

        public WindData GetWindDataAtTime(float time)           
        {
            if(dataPoints.Length > 1 && (directionSpline == null || speedSpline == null || cloudSpline == null))
            {
                CreateSplines();    
            }

            WindData w = dataPoints[0];     
            w.time = time;                  
            if(dataPoints.Length > 1)       
            {
                w.direction = directionSpline.GetYValue(time);
                w.speed = speedSpline.GetYValue(time) * WUIEngine.INPUT.Fire.windMultiplier;
                w.cloudCover = cloudSpline.GetYValue(time);
            }

            return w;
        }

        private void CreateSplines()                                                    
        {
            Vector2[] dir = new Vector2[dataPoints.Length];                             
            for (int i = 0; i < dir.Length; i++)
            {
                dir[i] = new Vector2(dataPoints[i].time, dataPoints[i].direction);
            }
            directionSpline = new CatmullRomSpline1D(dir);                                

            Vector2[] speed = new Vector2[dataPoints.Length];                           
            for (int i = 0; i < speed.Length; i++)
            {
                speed[i] = new Vector2(dataPoints[i].time, dataPoints[i].speed);
            }
            speedSpline = new CatmullRomSpline1D(speed);

            Vector2[] cloud = new Vector2[dataPoints.Length];                           
            for (int i = 0; i < cloud.Length; i++)
            {
                cloud[i] = new Vector2(dataPoints[i].time, dataPoints[i].cloudCover);
            }
            cloudSpline = new CatmullRomSpline1D(cloud);
        }

        public static WindInput GetTemplate()                                           
        {
            WindData[] w = new WindData[2];
            w[0] = new WindData(0f, 0f, 2f, 0f);
            w[1] = w[0];
            WindInput wI = new WindInput(w);
            return wI;
        }

        public static WindInput LoadWindInputFile(out bool success)
        {
            success = false;
            WindInput result = null;
            List<WindData> windData = new List<WindData>();

            string path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.windFile);
            bool fileExists = File.Exists(path);
            if (fileExists)
            {
                string[] dataLines = File.ReadAllLines(path);

                string[] header = dataLines[0].Split(',');
                header[0].Trim(' ');

                //header defined in Months, days, hour
                if(header[0] == "Month")
                {
                    //skip first line (header)
                    for (int j = 1; j < dataLines.Length; j++)
                    {
                        string[] data = dataLines[j].Split(',');                        

                        if(data.Length >= 6)
                        {
                            int month, day, hour, speed, direction, cloudCover;
                            bool b1 = int.TryParse(data[0], out month);
                            bool b2 = int.TryParse(data[1], out day);
                            bool b3 = int.TryParse(data[2], out hour);
                            bool b4 = int.TryParse(data[3], out speed);
                            bool b5 = int.TryParse(data[4], out direction);
                            bool b6 = int.TryParse(data[5], out cloudCover);

                            if (b1 && b2 && b3 && b4 && b5 && b6)
                            {
                                WindData wD = new WindData(month, day, hour, direction, speed, cloudCover);
                                windData.Add(wD);
                            }
                        }
                    }
                }
                //Header defined in seconds
                else if (header[0] == "Seconds")
                {
                    //skip first line (header)
                    for (int j = 1; j < dataLines.Length; j++)
                    {
                        string[] data = dataLines[j].Split(',');                        

                        if(data.Length >= 4)
                        {
                            int seconds, speed, direction, cloudCover;
                            bool b1 = int.TryParse(data[0], out seconds);
                            bool b2 = int.TryParse(data[1], out speed);
                            bool b3 = int.TryParse(data[2], out direction);
                            bool b4 = int.TryParse(data[3], out cloudCover);

                            if (b1 && b2 && b3 && b4)
                            {
                                WindData wD = new WindData(seconds, direction, speed, cloudCover);
                                windData.Add(wD);
                            }
                        }  
                    }
                }
                
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Wind data file " + path + " not found, will not be able to do fire or smoke spread simulations.");
            }

            if (windData.Count > 0)
            {
                result = new WindInput(windData.ToArray());
                success = true;
                WUIEngine.LOG(WUIEngine.LogType.Log, " Wind input data file " + path + " was found, " + windData.Count + " valid data points were succesfully loaded.");
            }
            else if (fileExists)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Wind input data file " + path + " was found but did not contain any valid data, will not be able to do fire or smoke spread simulations.");
            }

            return result;
        }
    }    
}
