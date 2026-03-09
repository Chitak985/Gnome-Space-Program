using Godot;
using System;

// Orbit
public class Orbit
{
    public CelestialBody parent;
    public CelestialBody cBody;
    public double MU;

    public double semiMajorAxis;
    public double eccentricity;
    public double inclination;
    public double argumentOfPeriapsis;
    public double longitudeOfAscendingNode;
    public double trueAnomaly;
    public double trueAnomalyAtEpoch;
    public double sphereOfInfluence;

    public double period;

    public double ComputeMU()
    {
        MU = Conics.GravConstant * parent.mass;
        return MU;
    }

    public double ComputePeriod()
    {
        period = 2 * Math.PI * Math.Sqrt(semiMajorAxis * semiMajorAxis * semiMajorAxis / MU); //Orbital period
        return period;
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
        Logger.Print("MU: " + MU);
        Logger.Print("----------------------------------");
    }

    public override string ToString()
    {
        return $"SMA: {Math.Round(semiMajorAxis, 2)}m \nECC: {Math.Round(eccentricity, 2)} \nINC: {Math.Round(inclination, 2)} \nARGP: {Math.Round(argumentOfPeriapsis, 2)} \nLONASC: {Math.Round(longitudeOfAscendingNode, 2)} \nTRUAN: {Math.Round(trueAnomaly, 2)} \nPERD: {Math.Round(period, 2)}";
    }
}