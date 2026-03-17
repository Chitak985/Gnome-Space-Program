using Godot;
using System;

// Orbit
public class Orbit
{
    public CelestialBody parent;
    public CelestialBody cBody;

    public double semiMajorAxis;
    public double eccentricity;
    public double inclination;
    public double argumentOfPeriapsis;
    public double longitudeOfAscendingNode;
    public double trueAnomaly;
    public double meanAnomalyAtEpoch;
    public double sphereOfInfluence;

    public double period;

    public double ComputeMU()
    {
        return Conics.GravConstant * parent.mass;
    }

    public double ComputePeriod()
    {
        return 2 * Math.PI * Math.Sqrt(semiMajorAxis * semiMajorAxis * semiMajorAxis / ComputeMU());
    }

    // Dump all orbit parameters to the console
    public void DumpOrbitParams()
    {
        Logger.Print("------ Orbit parameter dump ------");
        Logger.Print("Semimajor-axis: " + semiMajorAxis);
        Logger.Print("Eccentricity: " + eccentricity);
        Logger.Print("Inclination: " + inclination);
        Logger.Print("Argument Of Periapsis: " + argumentOfPeriapsis);
        Logger.Print("Longitude of Ascending Node: " + longitudeOfAscendingNode);
        Logger.Print("True Anomaly: " + trueAnomaly);
        Logger.Print("Period: " + period);
        Logger.Print("----------------------------------");
    }

    public override string ToString()
    {
        return $"SMA: {Math.Round(semiMajorAxis, 2)}m \nECC: {Math.Round(eccentricity, 2)} \nINC: {Math.Round(inclination, 2)} \nARGP: {Math.Round(argumentOfPeriapsis, 2)} \nLONASC: {Math.Round(longitudeOfAscendingNode, 2)} \nTRUAN: {Math.Round(trueAnomaly, 2)} \nPERD: {Math.Round(period, 2)}";
    }
}