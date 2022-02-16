using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public class GodCamera : MonoBehaviour
    {
        public enum CameraMode { twoD, threeD }
        CameraMode cMode = CameraMode.twoD;
        [SerializeField] float zoomSpeed = 100.0f;
        [SerializeField] Camera c;

        bool dragging = false;
        Vector3 startDragPos;
        Vector3 startMousePos;

        // Use this for initialization
        void OnValidate()
        {
            if(c == null)
            {
                c = GetComponent<Camera>();
            }            
        }

        public void SetCameraStartPosition(Vector2D mapSize)
        {
            float yPos = 0.5f * (float)mapSize.y / Mathf.Tan(Mathf.Deg2Rad * c.fieldOfView * 0.5f);

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
                SetCameraStartPosition(WUInity.INPUT.size);
            }
            if (cMode == CameraMode.twoD)
            {
                float d = Input.mouseScrollDelta.y;
                if (d != 0.0f)
                {
                    float mod = transform.position.y * 0.1f;
                    mod = Mathf.Max(1.0f, mod);
                    transform.position -= Vector3.up * Time.deltaTime * zoomSpeed * d * mod;
                    if (transform.position.y < 1.0f)
                    {
                        transform.position = new Vector3(transform.position.x, 1.0f, transform.position.z);
                    }
                    //rescale clip planes
                    c.farClipPlane = transform.position.y / Mathf.Sin(Mathf.PI * 0.5f - Mathf.Deg2Rad * c.fieldOfView * 0.5f) + 1.0f;
                    c.nearClipPlane = c.farClipPlane * 0.8f;
                }

                //add: limit to zoom when we have ground/terrain

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
                    float mapSize = 2.0f * transform.position.y / (Mathf.PI * 0.5f - Mathf.Sin(Mathf.Deg2Rad * c.fieldOfView * 0.5f));
                    Vector2 res = new Vector2(Screen.width, Screen.height);
                    transform.position = startDragPos + mapSize * (Vector3.left * (Input.mousePosition.x - startMousePos.x) / res.x + (res.y / res.x) * Vector3.back * (Input.mousePosition.y - startMousePos.y) / res.y);
                }
            }
            else
            {

            }

        }
    }
}