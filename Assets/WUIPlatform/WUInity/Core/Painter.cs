//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;

namespace WUIPlatform.WUInity
{
    public class Painter : MonoBehaviour
    {
        public enum PaintMode { EvacGroup, WUIArea, RandomIgnitionArea, InitialIgnition, PopulationMask };
        PaintMode paintMode = PaintMode.EvacGroup;

        Color currentColor = Color.red;
        const float transparency = 0.5f;
        Color activeAreaColor = new Color(1f, 0f, 0f, transparency);
        Color inactiveAreaColor = new Color(1f, 1f, 1f, 0.1f);
        Vector2int activeCellCount;
        Vector2d activeRealSize;
        Texture2D activeTexture;
        Color[] activeColorArray;
        private int _brushSize;

        //general evac stuff
        Vector2d evacDataRealSize;
        Vector2int evacDataCellCount;
        //evac groups
        Texture2D evacGroupTex;
        int evacGroupIndex;
        Color[] evacGroupColorArray;

        //general fire stuff
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

        //population mask painter
        Texture2D populationMaskTex;
        Color[] populationMaskColorArray;

        public PaintMode GetPaintMode()
        {
            return paintMode;
        }

        public Texture2D GetEvacGroupTexture()
        {
            if (evacGroupTex == null)
            {
                CheckDataResources(evacGroupTex, evacGroupColorArray);
            }
            return evacGroupTex;
        }

        public Texture2D GetPopulationMaskTexture()
        {
            if (populationMaskTex == null)
            {
                Texture2D tex = (Texture2D)WUIEngine.RUNTIME_DATA.Population.Visualizer.GetPopulationMaskTexture();
                if (tex != null)
                {
                    populationMaskTex = tex;
                }
                else
                {
                    CheckDataResources(populationMaskTex, populationMaskColorArray);
                }                
            }
            return populationMaskTex;
        }

        public Texture2D GetWUIAreaTexture()
        {
            if (wuiAreaTex == null)
            {
                CheckDataResources(wuiAreaTex, wuiAreaColorArray);
            }
            return wuiAreaTex;
        }

        public Texture2D GetRandomIgnitionTexture()
        {
            if (randomIgnitionTex == null)
            {
                CheckDataResources(randomIgnitionTex, randomIgnitionColorArray);
            }
            return randomIgnitionTex;
        }

        public Texture2D GetInitialIgnitionTexture()
        {
            if (initialIgnitionTex == null)
            {
                CheckDataResources(initialIgnitionTex, initialIgnitionColorArray);
            }
            return initialIgnitionTex;
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

        public void SetMaskGPWColor(bool addArea)
        {
            SetColor(addArea ? 1 : 0);
        }

        private void SetColor(int arrayIndex = 0)
        {
            if(paintMode == PaintMode.WUIArea || paintMode == PaintMode.RandomIgnitionArea || paintMode == PaintMode.InitialIgnition 
                || paintMode == PaintMode.PopulationMask)
            {
                currentColor = arrayIndex == 1 ? activeAreaColor : inactiveAreaColor;
                addingArea = arrayIndex == 1;
            }
            else if(paintMode == PaintMode.EvacGroup)
            {
                evacGroupIndex = arrayIndex;
                currentColor = WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGroups[evacGroupIndex].Color.UnityColor;
                currentColor.a = transparency;
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
            else if (mode == PaintMode.PopulationMask)
            {
                SetupPainterPopulationMask();
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Desired paint mode not yet implemented.");
            }
        }

        void SetupPainterEvacGroups()
        {
            paintMode = PaintMode.EvacGroup;
            CheckDataResources(evacGroupTex, evacGroupColorArray);
            //select first zone
            evacGroupIndex = 0;
            currentColor = WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGroups[evacGroupIndex].Color.UnityColor;
            currentColor.a = 0.5f;
            _brushSize = 1;
        }

        void SetupPainterPopulationMask()
        {
            paintMode = PaintMode.PopulationMask;
            CheckDataResources(populationMaskTex, populationMaskColorArray);
            //select first zone
            SetMaskGPWColor(true);
            _brushSize = 1;
        }

        void SetupPainterWUIArea()
        {
            paintMode = PaintMode.WUIArea;
            CheckDataResources(wuiAreaTex, wuiAreaColorArray);
            SetWUIAreaColor(true);
            _brushSize = 5;
        }

        void SetupPainterRandomIgnition()
        {
            paintMode = PaintMode.RandomIgnitionArea;
            CheckDataResources(randomIgnitionTex, randomIgnitionColorArray);
            SetRandomIgnitionAreaColor(true);
            _brushSize = 5;
        }
        void SetupPainterInitialIgnition()
        {
            paintMode = PaintMode.InitialIgnition;
            CheckDataResources(initialIgnitionTex, initialIgnitionColorArray);
            SetInitialIgnitionAreaColor(true);
            _brushSize = 3;
        }

        void CheckDataResources(Texture2D requestedTexture, Color[] requestedColorArray)
        {
            if (requestedTexture == null)
            {
                Vector2int cellCount;
                //get correct size, fire mesh or evac mesh
                if (paintMode == PaintMode.WUIArea || paintMode == PaintMode.RandomIgnitionArea || paintMode == PaintMode.InitialIgnition)
                {
                    if(WUIEngine.RUNTIME_DATA.Fire.LCPData != null)
                    {
                        fireDataCellCount = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCount();
                        cellCount = fireDataCellCount;
                        fireDataRealSize = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetSize();
                    }
                    else
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Warning, "Painter is trying to access LCP data but it is not loaded.");
                        return;
                    }
                }
                else
                {
                    //WUIEngine.SIM.UpdateNeededData();
                    evacDataCellCount = WUIEngine.RUNTIME_DATA.Evacuation.CellCount;
                    cellCount = evacDataCellCount;
                    evacDataRealSize = WUIEngine.INPUT.Simulation.Size;
                }
                //painter
                requestedColorArray = new Color[cellCount.x * cellCount.y];
                requestedTexture = new Texture2D(cellCount.x, cellCount.y);
                requestedTexture.filterMode = FilterMode.Point;
                for (int y = 0; y < cellCount.y; y++)
                {
                    for (int x = 0; x < cellCount.x; x++)
                    {
                        Color c = Color.white;
                        if (paintMode == PaintMode.WUIArea)
                        {
                            c = WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices[x + y * fireDataCellCount.x] == false ? inactiveAreaColor : activeAreaColor;
                        }
                        else if (paintMode == PaintMode.RandomIgnitionArea)
                        {
                            c = WUIEngine.RUNTIME_DATA.Fire.RandomIgnitionIndices[x + y * fireDataCellCount.x] == false ? inactiveAreaColor : activeAreaColor;
                        }
                        else if (paintMode == PaintMode.InitialIgnition)
                        {
                            c = WUIEngine.RUNTIME_DATA.Fire.InitialIgnitionIndices[x + y * fireDataCellCount.x] == false ? inactiveAreaColor : activeAreaColor;
                        }
                        else if (paintMode == PaintMode.EvacGroup)
                        {
                            c = WUIEngine.RUNTIME_DATA.Evacuation.GetEvacGroup(x, y).Color.UnityColor;
                            c.a = transparency;
                        }
                        else if (paintMode == PaintMode.PopulationMask)
                        {
                            c = WUIEngine.RUNTIME_DATA.Population.PopulationMap.Mask[x + y * evacDataCellCount.x] == true ? activeAreaColor : inactiveAreaColor;
                        }
                        requestedColorArray[x + y * cellCount.x] = c;
                        requestedTexture.SetPixel(x, y, c);
                    }
                }
                requestedTexture.Apply();              

                //fix references after created
                if (paintMode == PaintMode.WUIArea)
                {
                    wuiAreaTex = requestedTexture;
                    wuiAreaColorArray = requestedColorArray;
                }
                else if(paintMode == PaintMode.RandomIgnitionArea)
                {
                    randomIgnitionTex = requestedTexture;
                    randomIgnitionColorArray = requestedColorArray;
                }
                else if (paintMode == PaintMode.InitialIgnition)
                {
                    initialIgnitionTex = requestedTexture;
                    initialIgnitionColorArray = requestedColorArray;
                }
                else if (paintMode == PaintMode.EvacGroup)
                {
                    evacGroupTex = requestedTexture;
                    evacGroupColorArray = requestedColorArray;
                }
                else if (paintMode == PaintMode.PopulationMask)
                {
                    populationMaskTex = requestedTexture;
                    populationMaskColorArray = requestedColorArray;
                }
            }

            if (paintMode == PaintMode.WUIArea || paintMode == PaintMode.RandomIgnitionArea || paintMode == PaintMode.InitialIgnition)
            {                
                activeCellCount = fireDataCellCount;
                activeRealSize = fireDataRealSize;
            }
            else
            {
                activeCellCount = evacDataCellCount;
                activeRealSize = evacDataRealSize;
            }
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
            if(Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                ++_brushSize;
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                --_brushSize;
                if (_brushSize < 1)
                {
                    _brushSize = 1;
                }
            }
            if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(1))
            {
                Plane _yPlane = new Plane(Vector3.up, 0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector2 pixelUV = new Vector2(activeTexture.width * hitPoint.x / (float)activeRealSize.x, activeTexture.height * hitPoint.z / (float)activeRealSize.y);

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
                        Color colorToOverwrite = activeTexture.GetPixel(x, y);
                        FloodFill(new Vector2int(x, y), currentColor, colorToOverwrite, activeColorArray);
                        activeTexture.SetPixels(activeColorArray);
                        activeTexture.Apply();
                    }

                }
            }
        }    
        
        void PaintPixels(int x, int y)
        {
            if(_brushSize == 1)
            {
                activeTexture.SetPixel(x, y, currentColor);
                SetArrayPixel(x, y, currentColor, activeColorArray);
            }
            else
            {
                int minX = UnityEngine.Mathf.Max(0, x - _brushSize - 1);
                int maxX = UnityEngine.Mathf.Min(activeCellCount.x - 1, x + _brushSize - 1);
                int minY = UnityEngine.Mathf.Max(0, y - _brushSize - 1);
                int maxY = UnityEngine.Mathf.Min(activeCellCount.y - 1, y + _brushSize - 1);

                Vector2int center = new Vector2int(x, y);

                for (int j = minY; j <= maxY; j++)
                {
                    for (int i = minX; i <= maxX; i++)
                    {
                        float dist = Vector2int.Distance(center, new Vector2int(i, j));
                        if (dist <= _brushSize)
                        {
                            activeTexture.SetPixel(i, j, currentColor);
                            SetArrayPixel(i, j, currentColor, activeColorArray);
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
                WUIEngine.RUNTIME_DATA.Evacuation.EvacGroupIndices[x + y * activeCellCount.x] = evacGroupIndex;
            }
            else if(paintMode == PaintMode.WUIArea)
            {
                WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices[x + y * activeCellCount.x] = addingArea;
            }
            else if (paintMode == PaintMode.RandomIgnitionArea)
            {
                WUIEngine.RUNTIME_DATA.Fire.RandomIgnitionIndices[x + y * activeCellCount.x] = addingArea;
            }
            else if (paintMode == PaintMode.InitialIgnition)
            {
                WUIEngine.RUNTIME_DATA.Fire.InitialIgnitionIndices[x + y * activeCellCount.x] = addingArea;
            }
            else if (paintMode == PaintMode.PopulationMask)
            {
                WUIEngine.RUNTIME_DATA.Population.PopulationMap.Mask[x + y * activeCellCount.x] = addingArea;
            }
        }

        private void FloodFill(Vector2int startPixel, Color wantedColor, Color colorToOverwrite, Color[] colorArray)
        {   
            System.Collections.Generic.Stack<Vector2int> queue = new System.Collections.Generic.Stack<Vector2int>();
            queue.Push(startPixel);
            int maxPixels = colorArray.Length;
            while (queue.Count > 0)
            {
                Vector2int pixel = queue.Pop();
                SetArrayPixel(pixel.x, pixel.y, wantedColor, colorArray);

                Vector2int right = pixel + Vector2int.right;
                Vector2int left = pixel + Vector2int.left;
                Vector2int up = pixel + Vector2int.up;
                Vector2int down = pixel + Vector2int.down;

                // then we can either go east
                if (IncludePixel(right, wantedColor, colorToOverwrite, colorArray))
                {
                    queue.Push(right);
                }
                // west
                if (IncludePixel(left, wantedColor, colorToOverwrite, colorArray))
                {
                    queue.Push(left);
                }
                //north
                if (IncludePixel(up, wantedColor, colorToOverwrite, colorArray))
                {
                    queue.Push(up);
                }
                //south
                if (IncludePixel(down, wantedColor, colorToOverwrite, colorArray))
                {
                    queue.Push(down);
                }

                --maxPixels;
                if(maxPixels < 0)
                {
                    break;
                }
            }            
        }

        private bool IncludePixel(Vector2int pixelIndex, Color wantedColor, Color colorToOverwrite, Color[] colorArray)
        {
            //outside of texture
            if (pixelIndex.x < 0 || pixelIndex.x > (int)((activeTexture.width - 1) ) || pixelIndex.y < 0 || pixelIndex.y > (int)((activeTexture.height - 1)))
            {
                return false;
            }

            Color currentColor = GetArrayPixel(pixelIndex.x, pixelIndex.y, colorArray);

            //already the same color in pixel
            if (UnityEngine.Mathf.Approximately(currentColor.r, wantedColor.r) && UnityEngine.Mathf.Approximately(currentColor.g, wantedColor.g)
                && UnityEngine.Mathf.Approximately(currentColor.b, wantedColor.b))
            {
                return false;
            }

            //not color we want to overwrite
            if (!UnityEngine.Mathf.Approximately(currentColor.r, colorToOverwrite.r) && !UnityEngine.Mathf.Approximately(currentColor.g, colorToOverwrite.g)
                && !UnityEngine.Mathf.Approximately(currentColor.b, colorToOverwrite.b))
            {
                return false;
            }

            return true;
        }
    }
}