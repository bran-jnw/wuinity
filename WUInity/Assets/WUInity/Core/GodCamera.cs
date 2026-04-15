//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using PREACT;

namespace WUInity
{
    public class GodCamera : MonoBehaviour
    {
        public enum CameraMode { twoD, threeD }
        CameraMode cMode = CameraMode.twoD;
        [SerializeField] float zoomSpeed = 100.0f;
        [SerializeField] float lowestY = 200f;
        [SerializeField] Camera cam;
        [SerializeField] private LineRenderer _rtsSlection;

        float maximumY;
        bool dragging = false;
        Vector3 startDragPos;
        Vector3 startMousePos;
        PREACT.Math.Vector2d _mapSize;
        bool refreshClipPlanes = false;
        private PREACT.Input.PREACTInput _input;
        Engine _engine;
        WUInityManager _manager;

        // Use this for initialization
        void OnValidate()
        {
            if(cam == null)
            {
                cam = GetComponent<Camera>();
            }            
        }

        public void Awake()
        {
            _rtsSlection.positionCount = 4;
        }

        public void SetManager(WUInityManager manager)
        {
            _manager = manager;
            _engine = _manager.Engine;
        }

        public void SetInput(PREACT.Input.PREACTInput input)
        {
            _input = input;
            SetCameraSize(_input.Simulation.DomainSize);
            inactive = false;
        }

        bool inactive = true;
        public void SetToWebMercatorMode()
        {
            inactive = true;
            transform.position = new Vector3(0f, 200f, 0f);
        }

        float maxSizeOrtho;
        private void SetCameraSize(PREACT.Math.Vector2d mapSize)
        {
            _mapSize = mapSize;
            maxSizeOrtho = 0.5f * Mathf.Min((float)mapSize.x, (float)mapSize.y);
            cam.orthographicSize = maxSizeOrtho;
            transform.position = new Vector3(0.5f * (float)mapSize.x, 200f, 0.5f * (float)mapSize.y);
        }

        // Update is called once per frame
        void Update()
        {
            if (_input == null || inactive)
            {
                return;
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
                    float mapWidth = 2.0f * transform.position.y / (Mathf.PI * 0.5f - Mathf.Sin(Mathf.Deg2Rad * cam.fieldOfView * 0.5f));
                    Vector2 res = new Vector2(Screen.width, Screen.height);
                    transform.position = startDragPos + mapWidth * (Vector3.left * (Input.mousePosition.x - startMousePos.x) / res.x + (res.y / res.x) * Vector3.back * (Input.mousePosition.y - startMousePos.y) / res.y);                    
                }
                else
                {
                    float d = Input.mouseScrollDelta.y;
                    if (d != 0.0f)
                    {
                        float mod = transform.position.y * 0.1f;
                        mod = Mathf.Max(1.0f, mod);
                        cam.orthographicSize -= zoomSpeed * Mathf.Sign(d) * mod;
                        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 100f, maxSizeOrtho);
                    }
                }

                Vector3 clampedPos = transform.position;
                clampedPos.x = Mathf.Clamp(clampedPos.x, 0f, (float)_mapSize.x);
                clampedPos.y = 200f;
                clampedPos.z = Mathf.Clamp(clampedPos.z, 0f, (float)_mapSize.y);
                transform.position = clampedPos;
            }

            VehicleSelection();
        }

        Vector3 boundingBoxPos1, boundingBoxPos2, manualDestination;
        bool haveBoundingBox = false, selectingVehicles;
        Plane _yPlane = new Plane(Vector3.up, 0f);
        private void VehicleSelection()
        {
            //RTS stuff
            if (_engine.Simulation != null && _engine.Simulation.State == Simulation.SimulationState.Running && _engine.Simulation.IsPaused)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    selectingVehicles = false;
                    haveBoundingBox = false;
                    _rtsSlection.gameObject.SetActive(false);
                }

                if (selectingVehicles)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float enter;
                    if (_yPlane.Raycast(ray, out enter))
                    {
                        boundingBoxPos2 = ray.GetPoint(enter);
                        _rtsSlection.SetPosition(0, new Vector3(Mathf.Min(boundingBoxPos1.x, boundingBoxPos2.x), 10f, Mathf.Min(boundingBoxPos1.z, boundingBoxPos2.z)));
                        _rtsSlection.SetPosition(1, new Vector3(Mathf.Max(boundingBoxPos1.x, boundingBoxPos2.x), 10f, Mathf.Min(boundingBoxPos1.z, boundingBoxPos2.z)));
                        _rtsSlection.SetPosition(2, new Vector3(Mathf.Max(boundingBoxPos1.x, boundingBoxPos2.x), 10f, Mathf.Max(boundingBoxPos1.z, boundingBoxPos2.z)));
                        _rtsSlection.SetPosition(3, new Vector3(Mathf.Min(boundingBoxPos1.x, boundingBoxPos2.x), 10f, Mathf.Max(boundingBoxPos1.z, boundingBoxPos2.z)));
                        if (Input.GetMouseButtonUp(0))
                        {
                            haveBoundingBox = true;
                            selectingVehicles = false;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
                {
                    if (haveBoundingBox)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        float enter;
                        if (_yPlane.Raycast(ray, out enter))
                        {
                            manualDestination = ray.GetPoint(enter);
                            haveBoundingBox = false;
                            _manager.UpdateDestinationForVehicles(boundingBoxPos1, boundingBoxPos2, manualDestination);
                            _rtsSlection.gameObject.SetActive(false);
                        }
                    }
                    else if (!selectingVehicles)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        float enter;
                        if (_yPlane.Raycast(ray, out enter))
                        {
                            boundingBoxPos1 = ray.GetPoint(enter);
                            boundingBoxPos2 = boundingBoxPos1;
                            haveBoundingBox = false;
                            selectingVehicles = true;
                            _rtsSlection.SetPosition(0, boundingBoxPos1 + Vector3.up * 10);
                            _rtsSlection.SetPosition(1, boundingBoxPos1 + Vector3.up * 10);
                            _rtsSlection.SetPosition(2, boundingBoxPos1 + Vector3.up * 10);
                            _rtsSlection.SetPosition(3, boundingBoxPos1 + Vector3.up * 10);
                            _rtsSlection.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}