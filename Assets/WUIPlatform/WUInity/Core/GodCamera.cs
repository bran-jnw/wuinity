//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;

namespace WUIPlatform.WUInity
{
    public class GodCamera : MonoBehaviour
    {
        public enum CameraMode { twoD, threeD }
        CameraMode cMode = CameraMode.twoD;
        [SerializeField] float zoomSpeed = 100.0f;
        [SerializeField] float lowestY = 200f;
        [SerializeField] Camera c;

        float maximumY;
        bool dragging = false;
        Vector3 startDragPos;
        Vector3 startMousePos;
        WUIPlatform.Vector2d mapSize;
        bool refreshClipPlanes = false;

        // Use this for initialization
        void OnValidate()
        {
            if(c == null)
            {
                c = GetComponent<Camera>();
            }            
        }

        public void SetCameraStartPosition(WUIPlatform.Vector2d mapSize)
        {
            this.mapSize = mapSize;
            float yPos = 0.5f * (float)mapSize.y / Mathf.Tan(Mathf.Deg2Rad * c.fieldOfView * 0.5f);
            maximumY = yPos * 1.5f;

            transform.position = new Vector3((float)mapSize.x * 0.5f, yPos, (float)mapSize.y * 0.5f);
            //rescale clip planes
            c.farClipPlane = transform.position.y / Mathf.Sin(Mathf.PI * 0.5f - Mathf.Deg2Rad * c.fieldOfView * 0.5f) + 1.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (cMode == CameraMode.twoD)
                {
                    cMode = CameraMode.threeD;
                }
                else
                {
                    cMode = CameraMode.twoD;
                }
                SetCameraStartPosition(WUIPlatform.WUIEngine.INPUT.Simulation.Size);
            }

            if (cMode == CameraMode.twoD)
            {               

                if (Input.GetButtonDown("Fire3"))
                {
                    dragging = true;
                    startMousePos = Input.mousePosition;
                    startDragPos = transform.position;
                }
                else if (Input.GetButtonUp("Fire3"))
                {
                    dragging = false;
                }

                if (dragging)
                {
                    float mapWidth = 2.0f * transform.position.y / (Mathf.PI * 0.5f - Mathf.Sin(Mathf.Deg2Rad * c.fieldOfView * 0.5f));
                    Vector2 res = new Vector2(Screen.width, Screen.height);
                    transform.position = startDragPos + mapWidth * (Vector3.left * (Input.mousePosition.x - startMousePos.x) / res.x + (res.y / res.x) * Vector3.back * (Input.mousePosition.y - startMousePos.y) / res.y);                    
                }
                else
                {
                    float d = Input.mouseScrollDelta.y;
                    if (d != 0.0f || transform.position.y < lowestY)
                    {
                        float mod = transform.position.y * 0.1f;
                        mod = Mathf.Max(1.0f, mod);
                        transform.position -= Vector3.up * zoomSpeed * Mathf.Sign(d) * mod;
                        if (transform.position.y < lowestY)
                        {
                            transform.position = new Vector3(transform.position.x, lowestY, transform.position.z);
                        }
                        refreshClipPlanes = true;
                    }
                }

                Vector3 clampedPos = transform.position;
                clampedPos.x = Mathf.Clamp(clampedPos.x, 0f, (float)mapSize.x);
                clampedPos.y = Mathf.Clamp(clampedPos.y, lowestY, maximumY);
                clampedPos.z = Mathf.Clamp(clampedPos.z, 0f, (float)mapSize.y);
                transform.position = clampedPos;

                if(refreshClipPlanes)
                {
                    refreshClipPlanes = false;
                    //rescale clip planes
                    c.farClipPlane = transform.position.y / Mathf.Sin(Mathf.PI * 0.5f - Mathf.Deg2Rad * c.fieldOfView * 0.5f) + 1.0f;
                    c.nearClipPlane = c.farClipPlane * 0.8f;
                }
            }
            else
            {

            }

        }
    }
}