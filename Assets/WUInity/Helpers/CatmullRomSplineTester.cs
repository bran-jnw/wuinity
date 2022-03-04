using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRomSplineTester : MonoBehaviour
{
	public int samples = 20;
    public Vector2[] points;
	WUInity.CatmullRomSpline1D spline;

	// Start is called before the first frame update
	void Start()
    {
		points = new Vector2[5];
		points[0] = Vector2.zero;
        for (int i = 1; i < points.Length; i++)
        {
			Vector2 p = points[i - 1] + Vector2.right * Random.Range(10f, 20f) + Vector2.up * Random.Range(-20f, 20f);
			points[i] = p;
        }
        spline = new WUInity.CatmullRomSpline1D(points);
    }

	void OnDrawGizmos()
	{
		if(points == null || points.Length < 2)
        {
			return;
        }
		Gizmos.color = Color.white;

		float range = points[points.Length - 1].x - points[0].x;

		//Draw the Catmull-Rom spline between the points
		for (int i = 0; i < samples - 1; i++)
		{
			float time = i * (range / (samples - 1));
			float y = spline.GetYValue(time);
			Vector3 p0 = new Vector3(time, 0f, y) + transform.position;

			time = (i + 1) * (range/ (samples - 1));
			y = spline.GetYValue(time);
			Vector3 p1 = new Vector3(time, 0f, y) + transform.position;

			Gizmos.DrawLine(p0, p1);
		}
	}
}
