using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRomSplineTester : MonoBehaviour
{
	public int samples = 20;
	public System.Numerics.Vector2[] points;
    WUIEngine.CatmullRomSpline1D spline;

	// Start is called before the first frame update
	void Start()
    {
		System.Numerics.Vector2[] points = new System.Numerics.Vector2[5];
		points[0] = System.Numerics.Vector2.Zero;
        for (int i = 1; i < points.Length; i++)
        {
            System.Numerics.Vector2 p = points[i - 1] + System.Numerics.Vector2.UnitX * Random.Range(10f, 20f) + System.Numerics.Vector2.UnitY * Random.Range(-20f, 20f);
			points[i] = p;
        }
        spline = new WUIEngine.CatmullRomSpline1D(points);
    }

	void OnDrawGizmos()
	{
		if(points == null || points.Length < 2)
        {
			return;
        }
		Gizmos.color = Color.white;

		float range = points[points.Length - 1].X - points[0].X;

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
