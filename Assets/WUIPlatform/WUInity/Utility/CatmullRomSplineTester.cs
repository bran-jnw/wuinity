//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;

public class CatmullRomSplineTester : MonoBehaviour
{
	public int samples = 20;
	public System.Numerics.Vector2[] points;
    WUIPlatform.CatmullRomSpline1D spline;

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
        spline = new WUIPlatform.CatmullRomSpline1D(points);
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
