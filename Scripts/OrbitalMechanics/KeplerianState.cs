using Godot;
using System;

public partial class KeplerianState : Node
{
    public KeplerianElements elements;

    public struct KeplerianElements
    {
        public double semiMajorAxis;
        public double eccentricity;
        public double inclination;
        public double argumentOfPeriapsis;
        public double longitudeOfAscendingNode;
        public double trueAnomaly;
        public double meanAnomalyAtEpoch;
    }

    public override string ToString()
    {
        return 
        $"sma: {elements.semiMajorAxis} \necc: {elements.eccentricity} \ninc: {elements.inclination} \nargPer: {elements.argumentOfPeriapsis} \nlonAsc: {elements.longitudeOfAscendingNode} \ntruAn: {elements.trueAnomaly}";
    }
}
