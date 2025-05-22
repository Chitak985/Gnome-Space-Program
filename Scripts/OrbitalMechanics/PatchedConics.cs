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
    public static (Double3, Double3) KOEtoECI(Orbit orbit, CelestialBody parent, double time, double initialTime) //, Dr Freeman? Is it really that time again?
    {
        // yeah whatever the fRICK
        double MU = orbit.ComputeMU();//GravConstant * parent.mass;

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
            double PRD = orbit.ComputePeriod();//2 * Math.PI * Math.Sqrt(SMA * SMA * SMA / MU); //Orbital period
            double n = Math.Sqrt(MU/Math.Pow(SMA,3));
            double M = n*(time-initialTime);
            double EA = GetEccentricAnomaly(M, ECC);

            //double MA = EA - ECC * Math.Sin(EA);
            //V = 2 * Math.Atan(Math.Sqrt((1+ECC)/(1-ECC)) * Math.Tan(EA/2)); // V
            V = Math.Atan2(Math.Sqrt(1-Math.Pow(ECC,2)) * Math.Sin(EA), Math.Cos(EA) - ECC);
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
            double M = n*(time-initialTime);
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

    public static (Orbit, double) ECItoKOE(CartesianData data, CelestialBody parent, double initialTime)
    {
        Double3 pos = data.position;
        Double3 vel = data.velocity;

        // dumbass constant
        double MU = GravConstant * parent.mass;

        // Step one, compute specific angular momentum
        Double3 hBar = Double3.Cross(pos, vel);
        double h = hBar.Length();
        // Step two, compute radius
        double r = pos.Length();
        double v = vel.Length();
        // Step three, compute specific energy
        double E = (0.5 * Math.Pow(v, 2d))-(MU/r);
        // Step four, compute semi-major axis
        double a = -MU / (2*E);
        // Step five compute eccentricity yadda yadda also inclination which is part six
        double e = Math.Sqrt(1 - (Math.Pow(h,2) / (a * MU)));
        double i = Math.Acos(hBar.Z/h);
        // Step seven, right ascension or longitude of ascending node
        double omega_LAN = Math.Atan2(-hBar.X,hBar.Y);
        // Step EIGHT Argument of latitude
        double lat = Math.Atan2(-pos.Z / Math.Sin(i), pos.X * Math.Cos(omega_LAN) + pos.Y * Math.Sin(omega_LAN));
        if (i == 0) 
        {
            lat = Math.Atan2(-pos.X,pos.Y) + Math.PI/2;
        }else if (i == Math.PI){
            lat = Math.Atan2(-pos.X,-pos.Y) + Math.PI/2;
        }

        double p = a*(1-Math.Pow(e,2));
        double nu = Math.Atan2(Math.Sqrt(p/MU) * Double3.Dot(pos,vel), p-r);
        // Step ten argument of periapse ????????????/
        double omega_AP = nu + lat;
        // Step eleven, a meeting with an old friend known as "eccentric anomaly" or more commonly known as "electronic arts"
        double EA = 2*Math.Atan(Math.Sqrt((1-e)/(1+e)) * Math.Tan(nu/2));
        // Step twelve, we're done.
        double n = Math.Sqrt(MU/Math.Pow(a,3));
        double T = initialTime - 1/n * (EA - e * Math.Sin(EA));

        // wrap up all the newly created numbers with a neat bowtie
        // also invert semimajor axis for hyperbolic orbits
        if (a < 0) a *= -1;
        Orbit orbit = new()
        {
            parent = parent,
            semiMajorAxis = a,
            eccentricity = e, 
            inclination = i,
            argumentOfPeriapsis = omega_AP,
            longitudeOfAscendingNode = omega_LAN,
            trueAnomaly = nu,
            time = initialTime
        };
        orbit.ComputeMU();
        orbit.ComputePeriod();
        // return both orbit and time just in case time is ever needed
        return (orbit, T);
    }

    // Keplerian method of calculating eccentric anomaly apparently
    public static double GetEccentricAnomaly(double meanAnomaly, double eccentricity, double tolerance = 1e-2, int maxIter = 100000)
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

    // Solve for hyperbolic eccentric anomaly because that's DIFFERENT TOO?
    public static double GetHyperbolicAnomaly(double meanAnomaly, double eccentricity, double tolerance = 1e-2, int maxIter = 100000)
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

// Orbit
public class Orbit
{
    public CelestialBody parent;
    public double MU;

    public double semiMajorAxis;
    public double eccentricity;
    public double inclination;
    public double argumentOfPeriapsis;
    public double longitudeOfAscendingNode;
    public double trueAnomaly;
    public double time;

    public double period;

    public double ComputeMU()
    {
        MU = PatchedConics.GravConstant * parent.mass;
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
        GD.Print("------ Orbit parameter dump ------");
        GD.Print("Semimajor-axis: " + semiMajorAxis);
        GD.Print("Eccentricity: " + eccentricity);
        GD.Print("Inclination: " + inclination);
        GD.Print("Argument Of Periapsis: " + argumentOfPeriapsis);
        GD.Print("Longitude of Ascending Node: " + longitudeOfAscendingNode);
        GD.Print("True Anomaly: " + trueAnomaly);
        GD.Print("Time: " + time);
        GD.Print("Period: " + period);
        GD.Print("MU: " + MU);
        GD.Print("----------------------------------");
    }
}

// Cartesian data
public class CartesianData
{
    public CelestialBody parent;

    public Double3 position;
    public Double3 velocity;
}