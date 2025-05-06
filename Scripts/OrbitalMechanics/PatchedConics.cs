using Godot;
using System;

// All-encompassing class for orbital math
/*
Sources used:
https://en.wikipedia.org/wiki/Earth-centered_inertial
https://www.sciencedirect.com/topics/engineering/patched-conic
https://ai-solutions.com/_freeflyeruniversityguide/patched_conics_transfer.htm#calculatingapatchedconicsproblem
https://www.mathworks.com/help/aerotbx/ug/keplerian2ijk.html
https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
https://space.stackexchange.com/questions/24646/finding-x-y-z-vx-vy-vz-from-hyperbolic-orbital-elements

Handy list of orbital formulas:
https://www.bogan.ca/orbits/kepler/orbteqtn.html
*/
public partial class PatchedConics : Node
{
    public static float GravConstant {get;} = 6.674e-11F;
    // Gets body-centered coordinates from orbit parameters
    // Keplerian orbital elements to earth centered whateverthefuck
    public static (Double3, Double3) KOEtoECI(Orbit orbit, CelestialBody parent, double time, double initialTIme) //, Dr Freeman? Is it really that time again?
    {
        // Mathematicians are allergic to meaningful variable names so god knows what this is supposed to be
        double MU = GravConstant * parent.mass;

        // Compile our favourite Keplerian orbit elements
        double ECC = orbit.eccentricity;

        double SMA = orbit.semiMajorAxis;
        double INC = orbit.inclination;
        double AOP = orbit.argumentOfPeriapsis;
        double LAN = orbit.longitudeOfAscendingNode;
        //double TRA = orbit.trueAnomaly;

        double V;
        double radius;
        double H;
        double p;

        if (ECC < 1)
        {
            // Compute the mean anomaly
            double PRD = 2 * Math.PI * Math.Sqrt(SMA * SMA * SMA / MU); //Orbital period
            double n = Math.Sqrt(MU/Math.Pow(SMA,3));
            double M = n*(time-PRD);
            double EA = GetEccentricAnomaly(M, ECC);

            //double MA = EA - ECC * Math.Sin(EA);
            V = 2 * Math.Atan(Math.Sqrt((1+ECC)/(1-ECC)) * Math.Tan(EA/2)); // V
            // Step four
            radius = SMA * (1 - ECC*Math.Cos(EA));
            // PART FIVE! BOOBYTRAP THE LETTER H!!!
            H = Math.Sqrt(MU*SMA * (1 - Math.Pow(ECC,2)));
            // Step SIX!?!?? WHERE IS STEP SIX?
            // PART SEVEN!!!! ANGULAR MOMENTUM!
            p = SMA * (1 - Math.Pow(ECC,2));
        }else{
            // Compute the mean anomaly HYPERBOLIC EDITION
            double n = Math.Sqrt(MU/Math.Pow(Math.Abs(SMA),3));
            double M = n*time;
            double EA = GetHyperbolicAnomaly(M,ECC);

            // treu naomely
            
            H = Math.Sqrt(MU * Math.Abs(SMA) * (ECC * ECC - 1));
            V = 2 * Math.Atan(Math.Sqrt((ECC + 1) / (ECC - 1)) * Math.Tanh(EA / 2));
            p = Math.Abs(SMA) * (ECC * ECC - 1);
            radius = p / (1 + ECC * Math.Cos(V));
        }
        
        double Om = LAN;
        double w =  AOP;

        double X = radius*(Math.Cos(Om)*Math.Cos(w+V) - Math.Sin(Om)*Math.Sin(w+V)*Math.Cos(INC));
        double Y = radius*(Math.Sin(Om)*Math.Cos(w+V) + Math.Cos(Om)*Math.Sin(w+V)*Math.Cos(INC));
        double Z = radius*(Math.Sin(INC)*Math.Sin(w+V));

        double V_X = X*H*ECC/(radius*p)*Math.Sin(V) - H/radius*(Math.Cos(Om)*Math.Sin(w+V) + Math.Sin(Om)*Math.Cos(w+V)*Math.Cos(INC));
        double V_Y = Y*H*ECC/(radius*p)*Math.Sin(V) - H/radius*(Math.Sin(Om)*Math.Sin(w+V) - Math.Cos(Om)*Math.Cos(w+V)*Math.Cos(INC));
        double V_Z = Z*H*ECC/(radius*p)*Math.Sin(V) + H/radius*(Math.Cos(w+V)*Math.Sin(INC));

        return (new Double3(X,Y,Z), new Double3(V_X,V_Y,V_Z));
    }

    // Keplerian method of calculating eccentric anomaly apparently
    public static double GetEccentricAnomaly(double meanAnomaly, double eccentricity, double tolerance = 1e-8, int maxIter = 100000)
    {
        double E;

        if (eccentricity > 0.8){
            E = Math.PI;
        }else{
            E = meanAnomaly;
        }

        for (int i = 0; i < maxIter; i++)
        {
            double delta = (E - eccentricity * Math.Sin(E) - meanAnomaly) / (1 - eccentricity * Math.Cos(E));
            E -= delta;
            if (Math.Abs(delta) < tolerance)
            {
                break;
            }
        }
            
        return E;
    }

    // Solve for hyperbolic eccentric anomaly NOT H using Newton-Raphson
    public static double GetHyperbolicAnomaly(double meanAnomaly, double eccentricity, double tolerance = 1e-8, int maxIter = 100000)
    {
        double H = Math.Log(2 * Math.Abs(meanAnomaly) / eccentricity + 1.8); // Initial guess
        for (int i = 0; i < maxIter; i++)
        {
            double f = eccentricity * Math.Sinh(H) - H - meanAnomaly;
            double fp = eccentricity * Math.Cosh(H) - 1;
            double dH = f / fp;
            H -= dH;
            if (Math.Abs(dH) < tolerance)
                break;
        }
        return H;
    }
}

// Orbit without defining time.
public class Orbit
{
    public CelestialBody parent;

    public double semiMajorAxis;
    public double eccentricity;
    public double inclination;
    public double argumentOfPeriapsis;
    public double longitudeOfAscendingNode;
    public double trueAnomaly;

    public Double3 position;
    public Double3 velocity;
}
