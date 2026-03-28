using Godot;
using System;
using System.Collections.Generic;

public partial class OrbitRenderer : Node3D
{
    [Export] private Line2D line2D;
    [Export] private SubViewport viewport;
    [Export] private float margin = 1.1f;

    public OrbitDriver OrbitDriver;

    public bool enabled;

    public void Update()
    {
        Orbit orbit = OrbitDriver.orbit;

        if (orbit != null && OrbitDriver.enabled && false)
        {
            //Logger.Print(GetViewport().GetVisibleRect().Size * vpSizeRatio);

            viewport.Size = (Vector2I)MapView.Instance.Viewport.Size;
            //camDist = GetViewport().GetCamera3D().GlobalPosition.DistanceTo(GlobalPosition);
            double scale = orbit.semiMajorAxis * (1 + orbit.eccentricity) * margin;

            // Move the plane to match the orbit
            Scale = new Vector3(scale,scale,scale);
            GlobalRotation = new Vector3(-orbit.inclination, -orbit.longitudeOfAscendingNode, 0);

            List<Vector2> pointsList = SamplePoints(orbit);
            line2D.Points = [.. pointsList];
        }
    }

    // Sample multiple points in orbit
    public List<Vector2> SamplePoints(Orbit orbit)
    {
        double precision = OrbitRendererManager.Instance.orbitPrecision;

        int amount = (int)Math.Round(Math.PI * 2.0 * precision);
        if (orbit.eccentricity > 1)
            amount = (int)Math.Round(Math.Acos(-1 / orbit.eccentricity) / 2 * precision);

        double startTrueAn = orbit.trueAnomaly;

        List<Vector2> positions = [];

        for (int i = 0; i < amount; i++)
        {
            Orbit newOrbit = new()
            {
                parent = orbit.parent,
                semiMajorAxis = orbit.semiMajorAxis,
                eccentricity = orbit.eccentricity,
                inclination = orbit.inclination,
                argumentOfPeriapsis = orbit.argumentOfPeriapsis,
                longitudeOfAscendingNode = orbit.longitudeOfAscendingNode,
                trueAnomaly = startTrueAn + i/precision,
                period = orbit.period
            };
            CartesianData data = Conics.ElemToCart(newOrbit);
                
            Vector3 position = (data.position + orbit.parent.OrbitDriver.cartesian.position - MapView.Instance.FocusOffset) / MapView.Instance.ScaleFactor;

            Vector2 projectedPosition = GetViewport().GetCamera3D().UnprojectPosition(position);

            if (!GetViewport().GetCamera3D().IsPositionBehind(position))
            {
                positions.Add(projectedPosition);
            }
        }

        if (positions.Count > 0) positions.Add(positions[0]);

        return positions;
    }
}
