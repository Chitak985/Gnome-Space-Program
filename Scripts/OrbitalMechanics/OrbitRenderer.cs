using Godot;
using System;
using System.Collections.Generic;

public partial class OrbitRenderer : Line2D
{
	[Export] public bool enabled;
	[Export] public CelestialBody cBody;
	[Export] public Camera3D camera;

	[Export] public double precision = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (enabled)
		{
			//if (cBody.orbit.eccentricity > 1)
			//{
			//	Closed = false;
			//}else{
			//	Closed = true;
			//}

			List<Double3> points = SamplePoints(cBody, precision, camera);
			Vector2[] points2D = new Vector2[points.Count];
			for (int i = 0; i < points.Count; i++)
			{
				Double3 point = points[i];

				Vector3 floatPos = point.GetPosYUp().ToFloat3();

				Vector2 position = camera.UnprojectPosition(floatPos);

				points2D[i] = position;
			}
			Points = points2D;
		}
	}

	// Sample multiple points in orbit
	public static List<Double3> SamplePoints(CelestialBody body, double precision, Camera3D camera)
	{
		Orbit orbit = body.orbit;
		
		int amount = (int)Math.Round(Math.PI * 2.0 * precision);
		if (orbit.eccentricity > 1)
			amount = (int)Math.Round(Math.Acos(-1 / orbit.eccentricity) / 2 * precision);

		double startTrueAn = orbit.trueAnomaly;
		//if (orbit.eccentricity > 1)
		//	startTrueAn = -Math.Acos(-1 / orbit.eccentricity);

		List<Double3> positions = [];

		for (int i = 0; i < amount; i++)
		{
			Orbit newOrbit = new()
			{
				parent = orbit.parent,
				MU = orbit.MU,
				semiMajorAxis = orbit.semiMajorAxis,
				eccentricity = orbit.eccentricity,
				inclination = orbit.inclination,
				argumentOfPeriapsis = orbit.argumentOfPeriapsis,
				longitudeOfAscendingNode = orbit.longitudeOfAscendingNode,
				trueAnomaly = startTrueAn + i/precision,
				period = orbit.period
			};
            // Velocity is not used here so we discard it
            (Double3 position, _) = PatchedConics.KOEtoECI(newOrbit);
			//GD.Print(newOrbit.trueAnomaly);
			//GD.Print($"{position.X} {position.Y} {position.Z}");
			if (!camera.IsPositionBehind(position.GetPosYUp().ToFloat3()))
			{
				positions.Add(position);
			}//else{
			//	positions.Add(position);
			//}
        }

		return positions;
    }
}
