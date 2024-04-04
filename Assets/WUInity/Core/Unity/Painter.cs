using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public class Painter : MonoBehaviour
    {
        public enum PaintMode { EvacGroup, WUIArea, RandomIgnitionArea, InitialIgnition, CustomPopulation, TriggerBuffer };
        PaintMode paintMode = PaintMode.EvacGroup;

        Color activeColor = Color.red;
        Vector2 activeUV;
        Vector2int activeCellCount;
        Vector2d activeRealSize;
        Texture2D activeTexture;
        Color[] activeColorArray;
        int brushSize;

        //general evac stuff
        Vector2 evacDataUV;
        Vector2d evacDataRealSize;
        Vector2int evacDataCellCount;
        //evac groups
        Texture2D evacGroupTex;
        int evacGroupIndex;
        Color[] evacGroupColorArray;

        //general fire stuff
        Vector2 fireDataUV;
        Vector2d fireDataRealSize;
        Vector2int fireDataCellCount;
        bool addingArea;

        //wui area stuff
        Texture2D wuiAreaTex;
        Color[] wuiAreaColorArray;   
        
        //random ignition area stuff
        Texture2D randomIgnitionTex;
        Color[] randomIgnitionColorArray;

        //initial ignition
        Texture2D initialIgnitionTex;
        Color[] initialIgnitionColorArray;

        //initial ignition
        Texture2D triggerBufferTex;
        Color[] triggerBufferColorArray;

        //population mask painter
        Texture2D customPopulationMaskTex;
        Color[] customPopulationMaskColorArray;

        public PaintMode GetPaintMode()
        {
            return paintMode;
        }

        public Texture2D GetEvacGroupTexture()
        {
            if (evacGroupTex == null)
            {
                CheckDataResources(evacGroupTex, evacGroupColorArray, evacDataUV);
            }
            return evacGroupTex;
        }

        public Texture2D GetCustomPopulationMaskTexture()
        {
            if (customPopulationMaskTex == null)
            {
                CheckDataResources(customPopulationMaskTex, customPopulationMaskColorArray, evacDataUV);
            }
            return customPopulationMaskTex;
        }

        public Texture2D GetWUIAreaTexture()
        {
            if (wuiAreaTex == null)
            {
                CheckDataResources(wuiAreaTex, wuiAreaColorArray, fireDataUV);
            }
            return wuiAreaTex;
        }

        public Texture2D GetRandomIgnitionTexture()
        {
            if (randomIgnitionTex == null)
            {
                CheckDataResources(randomIgnitionTex, randomIgnitionColorArray, fireDataUV);
            }
            return randomIgnitionTex;
        }

        public Texture2D GetInitialIgnitionTexture()
        {
            if (initialIgnitionTex == null)
            {
                CheckDataResources(initialIgnitionTex, initialIgnitionColorArray, fireDataUV);
            }
            return initialIgnitionTex;
        }

        public Texture2D GetTriggerBufferTexture()
        {
            if (triggerBufferTex == null)
            {
                CheckDataResources(triggerBufferTex, triggerBufferColorArray, fireDataUV);
            }
            return triggerBufferTex;
        }

        public void SetEvacGroupColor(int groupIndex)
        {
            SetColor(groupIndex);
        }

        public void SetWUIAreaColor(bool addArea)
        {
            SetColor(addArea ? 1 : 0);
        }

        public void SetRandomIgnitionAreaColor(bool addArea)
        {
            SetColor(addArea ? 1 : 0);
        }

        public void SetInitialIgnitionAreaColor(bool addArea)
        {
            SetColor(addArea ? 1 : 0);
        }

        public void SetTriggerBufferColor(bool addArea)
        {
            SetColor(addArea ? 1 : 0);
        }

        public void SetCustomGPWColor(bool addArea)
        {
            SetColor(addArea ? 1 : 0);
        }

        private void SetColor(int arrayIndex = 0)
        {
            if(paintMode == PaintMode.WUIArea || paintMode == PaintMode.RandomIgnitionArea || paintMode == PaintMode.InitialIgnition 
                || paintMode == PaintMode.CustomPopulation || paintMode == PaintMode.TriggerBuffer)
            {
                activeColor = arrayIndex == 1 ? Color.red : Color.white;
                addingArea = arrayIndex == 1;
                activeColor.a = 0.5f;
            }
            else if(paintMode == PaintMode.EvacGroup)
            {
                evacGroupIndex = arrayIndex;
                activeColor = WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups[evacGroupIndex].Color.UnityColor;
                activeColor.a = 0.5f;
            }
        }

        public void SetPainterMode(PaintMode mode)
        {
            if (mode == PaintMode.EvacGroup)
            {
                SetupPainterEvacGroups();
            }
            else if (mode == PaintMode.WUIArea)
            {
                SetupPainterWUIArea();
            }
            else if (mode == PaintMode.RandomIgnitionArea)
            {
                SetupPainterRandomIgnition();
            }
            else if (mode == PaintMode.InitialIgnition)
            {
                SetupPainterInitialIgnition();
            }
            else if (mode == PaintMode.CustomPopulation)
            {
                SetupPainterCustomGPW();
            }
            else if (mode == PaintMode.TriggerBuffer)
            {
                SetupPainterTriggerBuffer();
            }
            else
            {
                WUInity.CONSOLE(WUInity.LogType.Error, "Desired paint mode not yet implemented.");
            }
        }

        void SetupPainterEvacGroups()
        {
            paintMode = PaintMode.EvacGroup;
            CheckDataResources(evacGroupTex, evacGroupColorArray, evacDataUV);
            //select first zone
            evacGroupIndex = 0;
            activeColor = WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups[evacGroupIndex].Color.UnityColor;
            activeColor.a = 0.5f;
            brushSize = 1;
        }

        void SetupPainterCustomGPW()
        {
            paintMode = PaintMode.CustomPopulation;
            CheckDataResources(customPopulationMaskTex, customPopulationMaskColorArray, evacDataUV);
            //select first zone
            SetCustomGPWColor(true);
            brushSize = 1;
        }

        void SetupPainterWUIArea()
        {
            paintMode = PaintMode.WUIArea;
            CheckDataResources(wuiAreaTex, wuiAreaColorArray, fireDataUV);
            SetWUIAreaColor(true);
            brushSize = 5;
        }

        void SetupPainterRandomIgnition()
        {
            paintMode = PaintMode.RandomIgnitionArea;
            CheckDataResources(randomIgnitionTex, randomIgnitionColorArray, fireDataUV);
            SetRandomIgnitionAreaColor(true);
            brushSize = 5;
        }
        void SetupPainterInitialIgnition()
        {
            paintMode = PaintMode.InitialIgnition;
            CheckDataResources(initialIgnitionTex, initialIgnitionColorArray, fireDataUV);
            SetInitialIgnitionAreaColor(true);
            brushSize = 3;
        }

        void SetupPainterTriggerBuffer()
        {
            paintMode = PaintMode.TriggerBuffer;
            CheckDataResources(triggerBufferTex, triggerBufferColorArray, fireDataUV);
            SetTriggerBufferColor(true);
            brushSize = 5;
        }

        void CheckDataResources(Texture2D requestedTexture, Color[] requestedColorArray, Vector2 requestedUV)
        {
            if (requestedTexture == null)
            {
                Vector2int cellCount;
                //get correct size, fire mesh or evac mesh
                if (paintMode == PaintMode.WUIArea || paintMode == PaintMode.RandomIgnitionArea || paintMode == PaintMode.InitialIgnition || paintMode == PaintMode.TriggerBuffer)
                {
                    fireDataCellCount = new Vector2int(WUInity.SIM.FireModule.GetCellCountX(), WUInity.SIM.FireModule.GetCellCountY());                    
                    cellCount = fireDataCellCount;
                    fireDataRealSize = WUInity.INPUT.Simulation.Size;
                }
                else
                {
                    //WUInity.SIM.UpdateNeededData();
                    evacDataCellCount = WUInity.RUNTIME_DATA.Evacuation.CellCount;
                    cellCount = evacDataCellCount;
                    evacDataRealSize = WUInity.INPUT.Simulation.Size;
                }
                //painter
                Vector2int res = new Vector2int(2, 2);
                while (cellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (cellCount.y > res.y)
                {
                    res.y *= 2;
                }
                requestedColorArray = new Color[res.x * res.y];
                requestedTexture = new Texture2D(res.x, res.y);
                requestedTexture.filterMode = FilterMode.Point;
                for (int y = 0; y < cellCount.y; y++)
                {
                    for (int x = 0; x < cellCount.x; x++)
                    {
                        Color c = Color.white;
                        if (paintMode == PaintMode.WUIArea)
                        {
                            c = WUInity.RUNTIME_DATA.Fire.WuiAreaIndices[x + y * fireDataCellCount.x] == false ? Color.white : Color.red;
                        }
                        else if (paintMode == PaintMode.RandomIgnitionArea)
                        {
                            c = WUInity.RUNTIME_DATA.Fire.RandomIgnitionIndices[x + y * fireDataCellCount.x] == false ? Color.white : Color.red;
                        }
                        else if (paintMode == PaintMode.InitialIgnition)
                        {
                            c = WUInity.RUNTIME_DATA.Fire.InitialIgnitionIndices[x + y * fireDataCellCount.x] == false ? Color.white : Color.red;
                        }
                        else if (paintMode == PaintMode.TriggerBuffer)
                        {
                            c = WUInity.RUNTIME_DATA.Fire.TriggerBufferIndices[x + y * fireDataCellCount.x] == false ? Color.white : Color.red;
                        }
                        else if (paintMode == PaintMode.EvacGroup)
                        {
                            c = WUInity.RUNTIME_DATA.Evacuation.GetEvacGroup(x, y).Color.UnityColor;
                        }
                        else if (paintMode == PaintMode.CustomPopulation)
                        {
                            c = WUInity.POPULATION.GetPopulationData().populationMask[x + y * evacDataCellCount.x] == true ? Color.red : Color.white;
                        }
                        c.a = 0.5f;
                        requestedColorArray[x + y * res.x] = c;
                        requestedTexture.SetPixel(x, y, c);
                    }
                }
                requestedTexture.Apply();
                requestedUV = new Vector2((float)cellCount.x / res.x, (float)cellCount.y / res.y);                

                //fix references after created
                if (paintMode == PaintMode.WUIArea)
                {
                    wuiAreaTex = requestedTexture;
                    wuiAreaColorArray = requestedColorArray;
                    fireDataUV = requestedUV;
                }
                else if(paintMode == PaintMode.RandomIgnitionArea)
                {
                    randomIgnitionTex = requestedTexture;
                    randomIgnitionColorArray = requestedColorArray;
                    fireDataUV = requestedUV;
                }
                else if (paintMode == PaintMode.InitialIgnition)
                {
                    initialIgnitionTex = requestedTexture;
                    initialIgnitionColorArray = requestedColorArray;
                    fireDataUV = requestedUV;
                }
                else if (paintMode == PaintMode.TriggerBuffer)
                {
                    triggerBufferTex = requestedTexture;
                    triggerBufferColorArray = requestedColorArray;
                    fireDataUV = requestedUV;
                }
                else if (paintMode == PaintMode.EvacGroup)
                {
                    evacGroupTex = requestedTexture;
                    evacGroupColorArray = requestedColorArray;
                    evacDataUV = requestedUV;
                }
                else if (paintMode == PaintMode.CustomPopulation)
                {
                    customPopulationMaskTex = requestedTexture;
                    customPopulationMaskColorArray = requestedColorArray;
                    evacDataUV = requestedUV;
                }
            }

            if (paintMode == PaintMode.WUIArea || paintMode == PaintMode.RandomIgnitionArea 
                || paintMode == PaintMode.InitialIgnition || paintMode == PaintMode.TriggerBuffer)
            {                
                activeCellCount = fireDataCellCount;
                activeRealSize = fireDataRealSize;
            }
            else
            {
                activeCellCount = evacDataCellCount;
                activeRealSize = evacDataRealSize;
            }
            activeUV = requestedUV;
            activeTexture = requestedTexture;
            activeColorArray = requestedColorArray;
        }      

        // Update is called once per frame
        void Update()
        {
            UpdatePainter();
        }

        void UpdatePainter()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(1))
            {
                Plane _yPlane = new Plane(Vector3.up, 0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector2 pixelUV = new Vector2(activeUV.x * activeTexture.width * hitPoint.x / (float)activeRealSize.x, activeUV.y * activeTexture.height * hitPoint.z / (float)activeRealSize.y);

                    int x = (int)pixelUV.x;
                    int y = (int)pixelUV.y;
                    if (x < 0 || x > activeCellCount.x - 1 || y < 0 || y > activeCellCount.y - 1)
                    {
                        return;
                    }

                    //left click
                    if(Input.GetMouseButton(0))
                    {
                        PaintPixels(x, y);
                    }
                    //right click
                    else
                    {
                        Color colorToOverwrite = activeTexture.GetPixel((int)pixelUV.x, (int)pixelUV.y);
                        flood_fill(x, y, activeColor, colorToOverwrite, activeColorArray);
                        activeTexture.SetPixels(activeColorArray);
                        activeTexture.Apply();
                    }

                }
            }
        }    
        
        void PaintPixels(int x, int y)
        {
            if(brushSize == 1)
            {
                activeTexture.SetPixel(x, y, activeColor);
                SetArrayPixel(x, y, activeColor, activeColorArray);
            }
            else
            {
                int minX = Mathf.Max(0, x - brushSize - 1);
                int maxX = Mathf.Min(activeCellCount.x - 1, x + brushSize - 1);
                int minY = Mathf.Max(0, y - brushSize - 1);
                int maxY = Mathf.Min(activeCellCount.y - 1, y + brushSize - 1);

                Vector2int center = new Vector2int(x, y);

                for (int j = minY; j <= maxY; j++)
                {
                    for (int i = minX; i <= maxX; i++)
                    {
                        float dist = Vector2int.Distance(center, new Vector2int(i, j));
                        if (dist <= brushSize)
                        {
                            activeTexture.SetPixel(i, j, activeColor);
                            SetArrayPixel(i, j, activeColor, activeColorArray);
                        }
                    }
                }
            }            
            
            activeTexture.Apply();
        }

        Color GetArrayPixel(int x, int y, Color[] colorArray)
        {
            return colorArray[x + y * activeTexture.width];
        }

        void SetArrayPixel(int x, int y, Color c, Color[] colorArray)
        {
            if (x < 0 || x > activeCellCount.x || y < 0 || y > activeCellCount.y)
            {
                return;
            }
            colorArray[x + y * activeTexture.width] = c;

            if(paintMode == PaintMode.EvacGroup)
            {
                WUInity.RUNTIME_DATA.Evacuation.EvacGroupIndices[x + y * activeCellCount.x] = evacGroupIndex;
            }
            else if(paintMode == PaintMode.WUIArea)
            {
                WUInity.RUNTIME_DATA.Fire.WuiAreaIndices[x + y * activeCellCount.x] = addingArea;
            }
            else if (paintMode == PaintMode.RandomIgnitionArea)
            {
                WUInity.RUNTIME_DATA.Fire.RandomIgnitionIndices[x + y * activeCellCount.x] = addingArea;
            }
            else if (paintMode == PaintMode.InitialIgnition)
            {
                WUInity.RUNTIME_DATA.Fire.InitialIgnitionIndices[x + y * activeCellCount.x] = addingArea;
            }
            else if (paintMode == PaintMode.TriggerBuffer)
            {
                WUInity.RUNTIME_DATA.Fire.TriggerBufferIndices[x + y * activeCellCount.x] = addingArea;
            }
            else if (paintMode == PaintMode.CustomPopulation)
            {
                WUInity.POPULATION.GetPopulationData().populationMask[x + y * activeCellCount.x] = addingArea;
            }
        }

        void flood_fill(int x, int y, Color wantedColor, Color colorToOverwrite, Color[] colorArray)
        {
            //outside of texture
            if (x < 0 || x > (int)((activeTexture.width - 1) * activeUV.x) || y < 0 || y > (int)((activeTexture.height - 1) * activeUV.y))
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
    }
}