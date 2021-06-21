using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public class EvacuationPainter : MonoBehaviour
    {
        //heneral stuff
        Vector2D size;
        Vector2 uv;
        Color c = Color.red;
        int width;
        int height;

        //evac goals
        Texture2D evacGoalTex;        
        EvacuationGoal evacGoal;        
        int evacGoalIndex = 0;
        Color[] evacGoalColorArray;

        //evac groups
        EvacGroup evacGroup;
        Texture2D evacGroupTex;
        int evacGroupIndex;
        Color[] evacGroupColorArray;

        public enum PaintMode {EvacGroup, ForceGoal };
        PaintMode paintMode = PaintMode.EvacGroup;
        

        void CheckForceMapData()
        {
            WUInityInput input = WUInity.WUINITY_IN;

            if (input.evac.evacuationForceTex == null)
            {
                //need to update cell size
                WUInity.WUINITY_SIM.UpdateNeededData();

                input.evac.paintedForcedGoals = new EvacuationGoal[input.evac.routeCellCount.x * input.evac.routeCellCount.y];                
                //painter
                Vector2Int res = new Vector2Int(2, 2);
                while (input.evac.routeCellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (input.evac.routeCellCount.y > res.y)
                {
                    res.y *= 2;
                }
                evacGoalColorArray = new Color[res.x * res.y];
                for (int y = 0; y < res.y; y++)
                {
                    for (int x = 0; x < res.x; x++)
                    {
                        Color c = input.traffic.evacuationGoals[0].color;
                        c.a = 0.5f;
                        evacGoalColorArray[x + y * res.x] = c;
                    }
                }
                
                input.evac.evacuationForceTex = new Texture2D(res.x, res.y);
                input.evac.evacuationForceTex.filterMode = FilterMode.Point;
                for (int y = 0; y < input.evac.routeCellCount.y; y++)
                {
                    for (int x = 0; x < input.evac.routeCellCount.x; x++)
                    {
                        Color c = input.traffic.evacuationGoals[0].color;
                        c.a = 0.5f;
                        input.evac.evacuationForceTex.SetPixel(x, y, c);
                        input.evac.paintedForcedGoals[x + y * input.evac.routeCellCount.x] = input.traffic.evacuationGoals[0];
                        
                    }
                }
                input.evac.evacuationForceTex.Apply();
                uv = new Vector2((float)input.evac.routeCellCount.x / res.x, (float)input.evac.routeCellCount.y / res.y);

                size = WUInity.WUINITY_IN.size;
                width = res.x;
                height = res.y;

                evacGoalTex = input.evac.evacuationForceTex;
            }
        }

        void CheckEvacGroupData()
        {
            WUInityInput input = WUInity.WUINITY_IN;

            if (input.evac.evacGroupTex == null)
            {
                //need to update cell size
                WUInity.WUINITY_SIM.UpdateNeededData();

                input.evac.paintedEvacGroups = new EvacGroup[input.evac.routeCellCount.x * input.evac.routeCellCount.y];                
                //painter
                Vector2Int res = new Vector2Int(2, 2);
                while (input.evac.routeCellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (input.evac.routeCellCount.y > res.y)
                {
                    res.y *= 2;
                }
                evacGroupColorArray = new Color[res.x * res.y];
                for (int y = 0; y < res.y; y++)
                {
                    for (int x = 0; x < res.x; x++)
                    {
                        Color c = input.evac.evacGroups[0].color;
                        c.a = 0.5f;
                        evacGroupColorArray[x + y * res.x] = c;
                    }
                }

                input.evac.evacGroupTex = new Texture2D(res.x, res.y);
                input.evac.evacGroupTex.filterMode = FilterMode.Point;
                for (int y = 0; y < input.evac.routeCellCount.y; y++)
                {
                    for (int x = 0; x < input.evac.routeCellCount.x; x++)
                    {
                        Color c = input.evac.evacGroups[0].color;
                        c.a = 0.5f;
                        input.evac.evacGroupTex.SetPixel(x, y, c);
                        input.evac.paintedEvacGroups[x + y * input.evac.routeCellCount.x] = input.evac.evacGroups[0];
                        evacGroupColorArray[x + y * input.evac.routeCellCount.x] = c;
                    }
                }
                input.evac.evacGroupTex.Apply();
                uv = new Vector2((float)input.evac.routeCellCount.x / res.x, (float)input.evac.routeCellCount.y / res.y);

                size = WUInity.WUINITY_IN.size;
                width = res.x;
                height = res.y;

                evacGroupTex = input.evac.evacGroupTex;
            }
        }

        void SetupPainterEvacGoals()
        {
            CheckForceMapData();

            //select first goal
            evacGoal = WUInity.WUINITY_IN.traffic.evacuationGoals[0];
            c = evacGoal.color;
            c.a = 0.5f;

            paintMode = PaintMode.ForceGoal;
        }

        void SetupPainterEvacGroups()
        {
            CheckEvacGroupData();

            //select first zone
            evacGroup = WUInity.WUINITY_IN.evac.evacGroups[0];
            c = evacGroup.color;
            c.a = 0.5f;

            paintMode = PaintMode.EvacGroup;
        }

        public void SetPainterMode(PaintMode mode)
        {
            if(mode == PaintMode.EvacGroup)
            {
                SetupPainterEvacGroups();

            }
            else if(mode == PaintMode.ForceGoal)
            {
                SetupPainterEvacGoals();
            }
        }

        // Update is called once per frame
        void Update()
        {
           if(paintMode == PaintMode.ForceGoal)
            {
                UpdateForceMap();
            }
           else if(paintMode == PaintMode.EvacGroup)
            {
                UpdateEvacGroup();
            }
        }

        void UpdateEvacGroup()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                ++evacGroupIndex;
                if (evacGroupIndex > WUInity.WUINITY_IN.evac.evacGroups.Length - 1)
                {
                    evacGroupIndex = 0;
                }
                evacGroup = WUInity.WUINITY_IN.evac.evacGroups[evacGroupIndex];
                c = evacGroup.color;
                c.a = 0.5f;
            }

            if (Input.GetMouseButton(0))
            {
                Plane _yPlane = new Plane(Vector3.up, 0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector2 pixelUV = new Vector2(uv.x * width * hitPoint.x / (float)size.x, uv.y * height * hitPoint.z / (float)size.y);

                    int x = (int)pixelUV.x;
                    int y = (int)pixelUV.y;
                    if (x < 0 || x > WUInity.WUINITY_IN.evac.routeCellCount.x || y < 0 || y > WUInity.WUINITY_IN.evac.routeCellCount.y)
                    {
                        return;
                    }

                    evacGroupTex.SetPixel(x, y, c);
                    SetArrayPixel(x, y, c, evacGroupColorArray);
                    evacGroupTex.Apply();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Plane _yPlane = new Plane(Vector3.up, 0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector2 pixelUV = new Vector2(uv.x * width * hitPoint.x / (float)size.x, uv.y * height * hitPoint.z / (float)size.y);

                    int x = (int)pixelUV.x;
                    int y = (int)pixelUV.y;
                    if (x < 0 || x > WUInity.WUINITY_IN.evac.routeCellCount.x || y < 0 || y > WUInity.WUINITY_IN.evac.routeCellCount.y)
                    {
                        return;
                    }

                    Color colorToOverwrite = evacGroupTex.GetPixel((int)pixelUV.x, (int)pixelUV.y);

                    flood_fill(x, y, c, colorToOverwrite, evacGroupColorArray);
                    evacGroupTex.SetPixels(evacGroupColorArray);
                    evacGroupTex.Apply();
                }
            }
        }

        void UpdateForceMap()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                ++evacGoalIndex;
                if (evacGoalIndex > WUInity.WUINITY_IN.traffic.evacuationGoals.Length - 1)
                {
                    evacGoalIndex = 0;
                }
                evacGoal = WUInity.WUINITY_IN.traffic.evacuationGoals[evacGoalIndex];
                c = evacGoal.color;
                c.a = 0.5f;
            }

            if (Input.GetMouseButton(0))
            {
                Plane _yPlane = new Plane(Vector3.up, 0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector2 pixelUV = new Vector2(uv.x * width * hitPoint.x / (float)size.x, uv.y * height * hitPoint.z / (float)size.y);

                    int x = (int)pixelUV.x;
                    int y = (int)pixelUV.y;
                    if (x < 0 || x > WUInity.WUINITY_IN.evac.routeCellCount.x || y < 0 || y > WUInity.WUINITY_IN.evac.routeCellCount.y)
                    {
                        return;
                    }

                    evacGoalTex.SetPixel(x, y, c);
                    SetArrayPixel(x, y, c, evacGoalColorArray);
                    evacGoalTex.Apply();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Plane _yPlane = new Plane(Vector3.up, 0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector2 pixelUV = new Vector2(uv.x * width * hitPoint.x / (float)size.x, uv.y * height * hitPoint.z / (float)size.y);

                    int x = (int)pixelUV.x;
                    int y = (int)pixelUV.y;
                    if (x < 0 || x > WUInity.WUINITY_IN.evac.routeCellCount.x || y < 0 || y > WUInity.WUINITY_IN.evac.routeCellCount.y)
                    {
                        return;
                    }

                    Color colorToOverwrite = evacGoalTex.GetPixel((int)pixelUV.x, (int)pixelUV.y);

                    flood_fill(x, y, c, colorToOverwrite, evacGoalColorArray);
                    evacGoalTex.SetPixels(evacGoalColorArray);
                    evacGoalTex.Apply();
                }
            }
        }

        Color GetArrayPixel(int x, int y, Color[] colorArray)
        {
            return colorArray[x + y * width];
        }

        void SetArrayPixel(int x, int y, Color c, Color[] colorArray)
        {
            if (x < 0 || x > WUInity.WUINITY_IN.evac.routeCellCount.x || y < 0 || y > WUInity.WUINITY_IN.evac.routeCellCount.y)
            {
                return;
            }
            colorArray[x + y * width] = c;

            if(paintMode == PaintMode.EvacGroup)
            {
                WUInity.WUINITY_IN.evac.paintedEvacGroups[x + y * WUInity.WUINITY_IN.evac.routeCellCount.x] = evacGroup;
            }
            else if(paintMode == PaintMode.ForceGoal)
            {
                WUInity.WUINITY_IN.evac.paintedForcedGoals[x + y * WUInity.WUINITY_IN.evac.routeCellCount.x] = evacGoal;
            }            
        }

        void flood_fill(int x, int y, Color wantedColor, Color colorToOverwrite, Color[] colorArray)
        {
            //outside of texture
            if (x < 0 || x > (int)((width - 1) * uv.x) || y < 0 || y > (int)((height - 1) * uv.y))
            {
                return;
            }

            Color currentColor = GetArrayPixel(x, y, colorArray);

            //already the same color in pixel
            if (Mathf.Approximately(currentColor.r, wantedColor.r) && Mathf.Approximately(currentColor.g, wantedColor.g)
                && Mathf.Approximately(currentColor.b, wantedColor.b))
            {
                return;
            }

            //not color we want to overwrite
            if (!Mathf.Approximately(currentColor.r, colorToOverwrite.r) && !Mathf.Approximately(currentColor.g, colorToOverwrite.g)
                && !Mathf.Approximately(colorToOverwrite.b, wantedColor.b))
            {
                return;
            }

            SetArrayPixel(x, y, wantedColor, colorArray);

            flood_fill(x + 1, y, wantedColor, colorToOverwrite, colorArray);  // then we can either go east
            flood_fill(x - 1, y, wantedColor, colorToOverwrite, colorArray);  // or west
            flood_fill(x, y + 1, wantedColor, colorToOverwrite, colorArray);  // or north
            flood_fill(x, y - 1, wantedColor, colorToOverwrite, colorArray);  // or south
        }

        /*void OnGUI()
        {
            int buttonHeight = 20;
            int buttonWidth = 100;
            WUInity.EvacuationGoal[] eG = WUInityManager.WUINITY_MANAGER.trafficOptions.evacuationGoals;
            for (int i = 0; i < eG.Length; ++i)
            {
                GUI.Box(new Rect(Screen.width - buttonWidth - 10, i * (buttonHeight + 5) + 10, buttonWidth, buttonHeight), eG[i].tex);
                GUI.Label(new Rect(Screen.width - buttonWidth - 10, i * (buttonHeight + 5) + 10, buttonWidth, buttonHeight), eG[i].name);
            }
        }*/
    }
}