using Godot;
using System;

// All-encompassing class for orbital math
/*
These are just saved links to some things i think might be useful because google is useless:
https://en.wikipedia.org/wiki/Earth-centered_inertial
https://www.sciencedirect.com/topics/engineering/patched-conic
https://ai-solutions.com/_freeflyeruniversityguide/patched_conics_transfer.htm#calculatingapatchedconicsproblem
https://www.mathworks.com/help/aerotbx/ug/keplerian2ijk.html
https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
https://space.stackexchange.com/questions/24646/finding-x-y-z-vx-vy-vz-from-hyperbolic-orbital-elements
https://space.stackexchange.com/questions/1904/how-to-programmatically-calculate-orbital-elements-using-position-velocity-vecto
https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf

list of orbital gobbledygook:
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
            p = SMA * (1 - Math.Pow(ECC,2));
            H = Math.Sqrt(MU * p);
        }else{
            // Compute the mean anomaly HYPERBOLIC EDITION
            double n = Math.Sqrt(MU/Math.Pow(Math.Abs(SMA),3));
            double M = n*(time-initialTime);
            double EA = GetHyperbolicAnomaly(M,ECC);

            // treu naomely
            
            p = Math.Abs(SMA) * (ECC * ECC - 1);
            H = Math.Sqrt(MU * p);
            V = 2 * Math.Atan(Math.Sqrt((ECC + 1) / (ECC - 1)) * Math.Tanh(EA / 2));
            
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

    // Converts position and velocity to classical Keplerian orbital elements.
    // Formulas taken from Basilisk https://hanspeterschaub.info/basilisk/_modules/orbitalMotion.html#rv2elem
    public static Orbit ECItoKOE(CartesianData data, CelestialBody parent, double initialTime)
    {
        // define mu, vectors, and epsilon
        double mu = GravConstant * data.parent.mass;
        Double3 rVec = data.position;
        Double3 vVec = data.velocity;
        double eps = 1e-8;

        // Specific angular momentum and its magnitude
        Double3 hVec = Double3.Cross(rVec,vVec);
        double h = hVec.Length();
        double p = h * h / mu;

        // The line of nodes (??)
        Double3 nVec = Double3.Cross(new Double3(0,0,1), hVec);
        double n = nVec.Length();

        // Orbit energy and eccentricity
        double r = rVec.Length();
        double v = vVec.Length();
        Double3 eVec = (v * v / mu - 1.0 / r) * rVec;
        Double3 v3 = Double3.Dot(rVec, vVec) / mu * vVec;
        eVec = eVec - v3;
        double e = eVec.Length();

        // Semimajor axis
        double alpha = 2.0 / r - v * v / mu;
        double a;
        if (Math.Abs(alpha) > eps)
        {
            // elliptic or hyperbolic
            a = 1.0 / alpha;
        }else{
            double rp = p / 2;
            a = -rp;
        }

        // Inclination
        double i = Math.Acos(hVec.Z / h);

        // The godless part.
        double Omega = 0; // Ascending node
        double omega = 0; // Arg. of periapsis
        double truAN = 0;
        if (e >= 1e-11 && i >= 1e-11)
        {
            // Non circular inclined orbit
            Omega = Math.Acos(nVec.X / n);
            if (nVec.Y < 0.0)
                Omega = 2.0 * Math.PI - Omega;
            omega = Math.Acos(Math.Clamp(Double3.Dot(nVec, eVec) / n / e, -1.0, 1.0));
            if (eVec.Z < 0.0)
                omega = 2.0 * Math.PI - omega;
            truAN = Math.Acos(Math.Clamp(Double3.Dot(eVec, rVec) / e / r, -1.0, 1.0));
            if (Double3.Dot(rVec, vVec) < 0.0)
                truAN = 2.0 * Math.PI - truAN;
        }else if (e >= 1e-11 && i < 1e-11)
        {
            // Non circular equatorial orbit
            // Equatorial orbit has no ascending node
            Omega = 0.0;
            // True longitude of periapsis
            omega = Math.Acos(eVec.X / e);
            if (eVec.Y < 0.0)
                omega = 2.0 * Math.PI - omega;
            truAN = Math.Acos(Math.Clamp(Double3.Dot(eVec, rVec) / e / r, -1.0, 1.0));
            if (Double3.Dot(rVec, vVec) < 0.0)
                truAN = 2.0 * Math.PI - truAN;   
        }else if (e < 1e-11 && i >= 1e-11)
        {
            // Circular inclined orbit
            Omega = Math.Acos(nVec.X / n);
            if (nVec.Y < 0.0)
                Omega = 2.0 * Math.PI - Omega;
            omega = 0.0;
            truAN = Math.Acos(Math.Clamp(Double3.Dot(nVec, rVec) / n / r, -1.0, 1.0));
            if (rVec.Z < 0.0)
                truAN = 2.0 * Math.PI - truAN;
        }else if (e < 1e-11 && i < 1e-11)
        {
            // Circular equatorial orbit
            Omega = 0.0;
            omega = 0.0;
            truAN = Math.Acos(rVec.X / r);
            if (rVec.Y < 0)
                truAN = 2.0 * Math.PI - truAN;
        }else{
            GD.PushError("Shit's fucked mate \n (Couldn't determine orbit type)");
        }

        if (e > 1.0 && Math.Abs(truAN) > Math.PI)
        {
            double twopiSigned = Math.CopySign(2.0 * Math.PI, truAN);
            truAN -= twopiSigned;
        }

        Orbit newOrbit = new()
        {
            parent = data.parent,
            semiMajorAxis = a,
            eccentricity = e,
            inclination = i,
            longitudeOfAscendingNode = Omega,
            argumentOfPeriapsis = omega,
            trueAnomaly = truAN,
        };

        return newOrbit;
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