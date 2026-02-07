using Godot;
using System;
using System.Collections.Generic;

public partial class OrbitRenderer : MeshInstance3D
{
    public const int MAX_ARRAY_LENGTH = 1024;
    public ShaderMaterial shaderMat;
    public Orbit orbit;
    public bool enabled;

    public override void _Ready()
    {
        ShaderMaterial mat = (ShaderMaterial)MaterialOverride.Duplicate(true);
        MaterialOverride = mat;
        shaderMat = (ShaderMaterial)MaterialOverride;
    }

    /*
    public override void _Process(double delta)
    {
        List<Vector3> pointsList = SamplePoints(50);
        shaderMat.SetShaderParameter("arrayLength", pointsList.Count);
        shaderMat.SetShaderParameter("points", pointsList.ToArray());

        // Move the plane to match the orbit
        Scale = new Vector3(orbit.semiMajorAxis, orbit.semiMajorAxis, orbit.semiMajorAxis) * (1.1 + orbit.eccentricity);
        Rotation = new Vector3(orbit.inclination, orbit.longitudeOfAscendingNode, 0);
    }
    */

    public void Update()
    {
        if (enabled && Visible)
        {
            // Move the plane to match the orbit
            Scale = new Vector3(orbit.semiMajorAxis, orbit.semiMajorAxis, orbit.semiMajorAxis) * (1.1 + orbit.eccentricity);
            Rotation = new Vector3(orbit.inclination, orbit.longitudeOfAscendingNode, 0);

            List<Vector3> pointsList = SamplePoints(50);
            shaderMat.SetShaderParameter("arrayLength", pointsList.Count);
            shaderMat.SetShaderParameter("points", pointsList.ToArray());
            shaderMat.SetShaderParameter("nodeSize", orbit.semiMajorAxis / ScaledSpace.Instance.ScaleFactor);
            shaderMat.SetShaderParameter("nodePosition", GlobalPosition);
        }else{
            Vector3[] bullshitArray = [Vector3.Zero];
            shaderMat.SetShaderParameter("arrayLength", 1);
            shaderMat.SetShaderParameter("points", bullshitArray);
        }
    }

    // Sample multiple points in orbit
    public List<Vector3> SamplePoints(double precision)
    {
        int amount = (int)Math.Round(Math.PI * 2.0 * precision);
        if (orbit.eccentricity > 1)
            amount = (int)Math.Round(Math.Acos(-1 / orbit.eccentricity) / 2 * precision);

        double startTrueAn = orbit.trueAnomaly;

        List<Vector3> positions = [];

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
                trueAnomaly = startTrueAn + i / precision,
                period = orbit.period
            };
            CartesianData data = Conics.ElemToCart(newOrbit);

            positions.Add(data.position / orbit.semiMajorAxis);
        }

        positions.Add(positions[0]);

        return positions;
    }
}
