using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OrbitRenderer : Line2D
{
	[Export] public bool enabled;
	[Export] public CelestialBody cBody;
	[Export] public Camera3D camera;

	// Sample amount for if no orbital period exists
	[Export] public int sampleAmnt = 100;
	[Export] public float sampleInterval = 0.1f;

	public double time;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (enabled)
		{
			if (cBody.orbit.eccentricity > 1)
			{
				Closed = false;
			}else{
				Closed = true;
			}

			int samples;
			if (cBody.orbit.period != 0)
			{
				samples = (int)(cBody.orbit.period / sampleInterval);
			}else{
				samples = sampleAmnt;
			}
			List<Double3> points = SamplePoints(cBody, samples, sampleInterval, cBody.orbit.initialTime);
			Vector2[] points2D = new Vector2[points.Count];
			for (int i = 0; i < points.Count; i++)
			{
				Double3 point = points[i];

				Vector3 floatPos = new(
					(float)point.X,
					(float)point.Y,
					(float)point.Z);

				Vector2 position = camera.UnprojectPosition(floatPos);

				points2D[i] = position;
			}
			Points = points2D;
		}
	}

	// Sample multiple points in orbit
	public static List<Double3> SamplePoints(CelestialBody body, int amount, float interval, double time)
	{
		Orbit orbit = body.orbit;

		List<Double3> positions = [];

		//double time = 0;

		for (int i = -1; i < amount; i++)
		{
			time += interval;
            // Velocity is not used here so we discard it
            (Double3 position, _) = PatchedConics.KOEtoECI(orbit, orbit.parent, time, 0);
			positions.Add(position);
        }

		return positions;
    }
}
