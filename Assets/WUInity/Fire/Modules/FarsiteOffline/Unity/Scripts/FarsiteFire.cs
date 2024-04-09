using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Numerics;

namespace WUIEngine.Farsite
{   
    public class FireVertex
    {
        public Vector3d pos;
        public FireVertex prev;
        public FireVertex next;

        public FireVertex()
        {
            //pos = Vector3D.zero;
            prev = null;
            next = null;
        }

        /*~FireVertex()
        {
            pos = Vector3D.zero;
            prev = null;
            next = null;
        }*/
    }


    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(BoxCollider))]
    public class FarsiteFire : MonoBehaviour
    {
        public RectTransform arrow;
        public double windSpeed = 0.0f;
        Vector2 startPoint;
        //List<Vector2> currentVertices;

        public int fireNumber = 0;
        List<FarsiteFire> overlappingFires = new List<FarsiteFire>();

        //
        Vector2 radius = new Vector2(1.0f, 1.0f);
        public float width = 0.05f;
        public int resolution = 32;

        private FireVertex[] fireVertices;
        private bool[] dead; 
        private LineRenderer self_lineRenderer;
        private BoxCollider bC;

        Vector2d min;
        Vector2d max;
        FarsiteViewer fR;

        private void Start()
        {
            bC = GetComponent<BoxCollider>();
            bC.isTrigger = true;
            
            min.x = float.MaxValue;
            min.y = float.MaxValue;
            max.x = float.MinValue;
            max.y = float.MinValue;

            //fR = WUInity.FARSITE_VIEWER;                       

            InitiateEllipse();
        }


        private void FixedUpdate()
        {
            if(Input.GetButton("Fire1"))
            {
                UpdateEllipse();
            }                      
        }
        
        void InitiateEllipse()
        {
            //stuff for line renderer
            self_lineRenderer = GetComponent<LineRenderer>();
            self_lineRenderer.loop = true;
            self_lineRenderer.useWorldSpace = true;
            self_lineRenderer.positionCount = resolution;
            self_lineRenderer.startWidth = width;
            self_lineRenderer.endWidth = width;

            //actual fire stuff
            fireVertices = new FireVertex[resolution];
            dead = new bool[resolution];


            Vector3d offset = new Vector3d((double)transform.position.x, (double)transform.position.y, (double)transform.position.z);
            for (int i = 0; i < resolution; ++i)
            {
                double angle = 2.0 * Math.PI * (double)i / (double)resolution;
                fireVertices[i] = new FireVertex();
                //swapped cos sin due to azimuth has origin at north then go clockwise
                fireVertices[i].pos = new Vector3d(radius.x * Math.Sin(angle), 0.0, radius.y * Math.Cos(angle));
                fireVertices[i].pos += offset;
                Vector3 pointPosition = new Vector3((float)fireVertices[i].pos.x, (float)fireVertices[i].pos.y, (float)fireVertices[i].pos.z);
                self_lineRenderer.SetPosition(i, pointPosition);
                dead[i] = false;               
            }

            //fill in neighbors
            for(int i = 0; i < fireVertices.Length; ++i)
            {
                if (i == 0)
                {
                    fireVertices[i].prev = fireVertices[fireVertices.Length - 1];
                    fireVertices[i].next = fireVertices[i + 1];
                }
                else if (i == fireVertices.Length - 1)
                {
                    fireVertices[i].prev = fireVertices[i - 1];
                    fireVertices[i].next = fireVertices[0];
                }
                else
                {
                    fireVertices[i].prev = fireVertices[i - 1];
                    fireVertices[i].next = fireVertices[i + 1];
                }
            }
        }

        public void UpdateEllipse()
        {
            Vector3d[] newPos = new Vector3d[resolution];
            for (int i = 0; i < resolution; ++i)
            {
                //where is the vertex located on the ellipse
                //if(!dead[i])
                //{
                    CalcEllipse(newPos, i);
                //}
            }
            bC.center = Vector3.right * (float)(max.x + min.x) * 0.5f + Vector3.forward * (float)(max.y + min.y) * 0.5f;
            bC.size = Vector3.right * (float)(max.x - min.x) + Vector3.forward * (float)(max.y - min.y) + Vector3.up;

            //actually move the vertices
            for (int i = 0; i < resolution; ++i)
            {
                fireVertices[i].pos = newPos[i];
                self_lineRenderer.SetPosition(i, new Vector3((float)fireVertices[i].pos.x, (float)fireVertices[i].pos.y, (float)fireVertices[i].pos.z));
            }

            return;

            //check for crossovers and mergers
            List<int> crossovers = new List<int>();
            //in mergers x is index of overlapping vertex, y is the number of the other fire
            List<Vector2Int> mergers = new List<Vector2Int>();
            for (int i = 0; i < resolution; ++i)
            {
                int g = fR.CheckFireGrid(fireNumber, fireVertices[i].pos.x, fireVertices[i].pos.z);
                //we have hit our own fire ring
                if(g == fireNumber)
                {
                    crossovers.Add(i);
                }
                //we have hit some other fire
                else if(g != 0 && g != fireNumber)
                {
                    dead[i] = true;
                    mergers.Add(new Vector2Int(i, g));
                }
            }

            ResolveCrossovers(crossovers);
            ResolveMergers(mergers);
        }

        void ResolveCrossovers(List<int> vertices)
        {

        }

        void ResolveMergers(List<Vector2Int> mergers)
        {

        }

        void CalcEllipse(Vector3d[] newPos, int index)
        {
            //stuff for using mouse as wind inpput
            Vector3 relPos = new Vector3(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2, 0.0f);
            relPos.Normalize();
            float angle = Vector3.Angle(relPos, Vector3.up);
            if(relPos.x < 0.0f)
            {
                angle = 360.0f - angle;
            }
            arrow.position = Input.mousePosition;
            arrow.up = relPos;
            //blow from opposite direction since it makes more sense to go from mouse pos to center
            angle -= 180.0f;            
            //Debug.Log("Angle: " + angle);
            angle = 2.0f * Mathf.PI * angle / 360.0f;

            //fetch wind data from raster
            //Vector2 windData = fR.GetWind(fireVertices[index].pos);
            //windSpeed = 1.0 + windData.x * 9.0f;
            //angle += 2.0f * (windData.y - 0.5f) * 0.125f * Mathf.PI;
                       
            //the same as found in paper and fsxwmech.cpp, Meachanix::ellipse
            double LB = 0.936 * exp(0.2566 * windSpeed) + 0.461 *exp(-0.1548 * windSpeed) - 0.397;
            // maximum eccentricity
            if (LB > 8.0)
            {
                LB = 8.0;
            }                
            double HB = (LB + sqrt(pow2(LB) - 1)) / (LB - sqrt(pow2(LB) - 1));
            double R = 10.0 / 60.0; //spreadrate m/s
            double a = 0.5 * (R + R / HB) / LB;
            double b = 0.5 * (R + R / HB);
            double c = b - R / HB;
            double phi = angle; 
            double x_s = Get_x_s(index);
            double y_s = Get_y_s(index);

            double part1, part2, part3, part4, part5, part6, f2, h2;
            f2 = pow2(a);
            h2 = pow2(b);
            part1 = f2 * cos(phi) * (x_s * sin(phi) + y_s * cos(phi));
            part2 = h2 * sin(phi) * (x_s * cos(phi) - y_s * sin(phi));
            part3 = h2 * pow2(x_s * cos(phi) - y_s * sin(phi));
            part4 = f2 * pow2(x_s * sin(phi) + y_s * cos(phi));
            part5 = f2 * sin(phi) * (x_s * sin(phi) + y_s * cos(phi));
            part6 = h2 * cos(phi) * (x_s * cos(phi) - y_s * sin(phi));
            double X_t = ((part1 - part2) / sqrt((part3 + part4))) + c * sin(phi);
            double Y_t = ((-part5 - part6) / sqrt((part3 + part4))) + c * cos(phi);

            //JW use vectors instead. edit does not work at the moment
            /*float dist = sqrt(pow2(X_t) + pow2(Y_t));
            Vector3D vec = GetVectorDir(index);
            vec.Normalize();
            Vector3D normal = Vector3D.Cross(Vector3.up, vec);*/

            newPos[index] = fireVertices[index].pos + new Vector3d(X_t, 0.0, Y_t) * System.Convert.ToDouble(Time.fixedDeltaTime) * fR.timeScale;            

            //update bounding box bounds
            if(fireVertices[index].pos.x < min.x)
            {
                min.x = fireVertices[index].pos.x;
            }
            if (fireVertices[index].pos.x > max.x)
            {
                max.x = fireVertices[index].pos.x;
            }
            if (fireVertices[index].pos.z < min.y)
            {
                min.y = fireVertices[index].pos.z;
            }
            if (fireVertices[index].pos.z > max.y)
            {
                max.y = fireVertices[index].pos.z;
            }            
        }

        int IntersectionCount(FarsiteFire fire2)
        {
            bool lastPointWasIn = false;
            for(int i = 0; i < fireVertices.Length; ++i)
            {

            }
            return 0;
        }

        Vector3d GetVectorDir(int index)
        {
            int prev = index - 1;
            if (prev < 0)
            {
                prev = resolution - 1;
            }

            int next = index + 1;
            if (next > resolution - 1)
            {
                next = 0;
            }

            Vector3d vec = fireVertices[next].pos - fireVertices[prev].pos;

            return vec;
        }

        double Get_x_s(int index)
        {
            return fireVertices[index].prev.pos.x - fireVertices[index].next.pos.x;

            int prev = index - 1;
            if(prev < 0)
            {
                prev = resolution - 1;
            }

            int next = index + 1;
            if (next > resolution - 1)
            {
                next = 0;
            }

            double x_s = fireVertices[prev].pos.x - fireVertices[next].pos.x;

            return x_s;
        }         

        double Get_y_s(int index)
        {
            return fireVertices[index].prev.pos.z - fireVertices[index].next.pos.z;

            int prev = index - 1;
            if (prev < 0)
            {
                prev = resolution - 1;
            }

            int next = index + 1;
            if (next > resolution - 1)
            {
                next = 0;
            }

            double y_s = fireVertices[prev].pos.z - fireVertices[next].pos.z;

            return y_s;
        }

        double sqrt(double x)
        {
            return Math.Sqrt(x);
        }

        double pow2(double x)
        {
            return x * x;
        }

        double sin(double x)
        {
            return Math.Sin(x);
        }

        double cos(double x)
        {
            return Math.Cos(x);
        }

        double exp(double x)
        {
            return Math.Exp(x);
        }

        private void OnTriggerEnter(Collider other)
        {
            FarsiteFire fF = other.GetComponent<FarsiteFire>();
            if(fireNumber > fF.fireNumber)
            {
                Debug.Log("Start overlapping fire AABB.");
            }
        }
    }
}

