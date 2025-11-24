using Godot;
using System;
using System.Collections.Generic;

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

public partial class Conics : Node
{
    // Buncha constants
    public static readonly double GravConstant = 6.674e-11;
    public static readonly double EarthGravity = 9.80665;
    
    // Functions to get points with Y as up rather than Z
    // To Be Eliminated
    private static Vector3 GetPosYUp(Vector3 inputVector)
    {
        return new Vector3(inputVector.X,inputVector.Z,inputVector.Y);
    }
    
    // Orbital Elements to Cartesian (relative to parent)
    public static CartesianData ElemToCart(Orbit orbit)
    {
        double MU = orbit.ComputeMU(); //GravConstant * parent.mass;

        // Compile our favourite Keplerian orbit elements
        double semiMajorAxis = orbit.semiMajorAxis;
        double eccentricity = orbit.eccentricity;
        double inclination = orbit.inclination;
        double argumentOfPeriapsis = orbit.argumentOfPeriapsis;
        double longitudeOfAscendingNode = orbit.longitudeOfAscendingNode;
        double trueAnomaly = orbit.trueAnomaly;

        double semiLatusRectum = eccentricity != 1.0 ? semiMajorAxis * (1 - eccentricity * eccentricity) : 2 * semiMajorAxis;
        double radius = semiLatusRectum / (1 + eccentricity * Math.Cos(trueAnomaly));
        //double specificAngularMomemtum = Math.Sqrt(MU * semiLatusRectum);

        Vector3 rPQW = new(
            radius * Math.Sin(trueAnomaly),
            0,
            radius * Math.Cos(trueAnomaly)
        );

        Vector3 vPQW = new(
            -Math.Sqrt(MU / semiLatusRectum) * Math.Sin(trueAnomaly),
            0,
            Math.Sqrt(MU / semiLatusRectum) * (eccentricity + Math.Cos(trueAnomaly))
        );

        Basis R =
            new Basis(Vector3.Up, (float)longitudeOfAscendingNode) * // Rotate around global Z
            new Basis(Vector3.Right, (float)inclination) *           // Rotate by inclination
            new Basis(Vector3.Up, (float)argumentOfPeriapsis);       // Rotate by argument of periapsis

        CartesianData data = new()
        {
            position = R * rPQW,
            velocity = R * vPQW
        };

        return data;
    }

    // Converts position and velocity to classical Keplerian orbital elements.
    // Formulas taken from Basilisk https://hanspeterschaub.info/basilisk/_modules/orbitalMotion.html#rv2elem
    public static Orbit CartToElem(CartesianData data)
    {
        // define mu, vectors, and epsilon
        double mu = GravConstant * data.parent.mass;
        Vector3 rVec = data.position;
        Vector3 vVec = data.velocity;
        double eps = 1e-8;

        // Specific angular momentum and its magnitude
        Vector3 hVec = rVec.Cross(vVec);
        double h = hVec.Length();
        double p = h * h / mu;

        // The line of nodes (??)
        // Back is (0, 0, 1)
        Vector3 nVec = Vector3.Back.Cross(hVec);
        //Double3 nVec = Double3.Cross(new Double3(0,0,1), hVec);
        double n = nVec.Length();

        // Orbit energy and eccentricity
        double r = rVec.Length();
        double v = vVec.Length();
        Vector3 eVec = (v * v / mu - 1.0 / r) * rVec;
        Vector3 v3 = rVec.Dot(vVec) / mu * vVec;
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
        if (e >= 1e-11 && i >= 1e-11 && i <= Math.PI - 1e-11)
        {
            // Non circular inclined orbit
            Omega = Math.Acos(nVec.X / n);
            if (nVec.Y < 0.0)
                Omega = 2.0 * Math.PI - Omega;
            omega = Math.Acos(Math.Clamp(nVec.Dot(eVec) / n / e, -1.0, 1.0));
            if (eVec.Z < 0.0)
                omega = 2.0 * Math.PI - omega;
            truAN = Math.Acos(Math.Clamp(eVec.Dot(rVec) / e / r, -1.0, 1.0));
            if (rVec.Dot(vVec) < 0.0)
                truAN = 2.0 * Math.PI - truAN;
        }else if (e >= 1e-11 && (i < 1e-11 || i > Math.PI - 1e-11))
        {
            // Non circular equatorial orbit
            // Equatorial orbit has no ascending node
            Omega = 0.0;
            // True longitude of periapsis
            omega = Math.Acos(eVec.X / e);
            // Handle cases where the orbit is retrograde
            if (i <= Math.PI - 1e-11)
            {
                if (eVec.Y < 0.0)
                    omega = 2.0 * Math.PI - omega;
            }else{
                if (eVec.Y > 0.0)
                    omega = 2.0 * Math.PI - omega;
            }

            truAN = Math.Acos(Math.Clamp(eVec.Dot(rVec) / e / r, -1.0, 1.0));
            if (rVec.Dot(vVec) < 0.0)
                truAN = 2.0 * Math.PI - truAN;   
        }else if (e < 1e-11 && i >= 1e-11)
        {
            // Circular inclined orbit
            Omega = Math.Acos(nVec.X / n);
            if (nVec.Y < 0.0)
                Omega = 2.0 * Math.PI - Omega;
            omega = 0.0;
            truAN = Math.Acos(Math.Clamp(nVec.Dot(rVec) / n / r, -1.0, 1.0));
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

    // Name is a bit confusing but all this does is convert time (t) to true anomaly (v)
    public static double TimeToTrueAnomaly(Orbit orbit, double t, double T)
    {
        double MU = orbit.MU;
        double v = 0;
        if (orbit.eccentricity > 1)
        {
            // Hyperbolic case
            double n = Math.Sqrt(MU/Math.Pow(Math.Abs(orbit.semiMajorAxis),3));
            double M = n*(t-T);
            double EA = GetHyperbolicAnomaly(M,orbit.eccentricity);

            v = 2 * Math.Atan(Math.Sqrt((orbit.eccentricity + 1) / (orbit.eccentricity - 1)) * Math.Tanh(EA / 2));
        }else{
            // Parabolic case
            double PRD = orbit.ComputePeriod();
            double n = Math.Sqrt(MU/Math.Pow(orbit.semiMajorAxis,3));
            double M = n*(t-T);
            double EA = GetEccentricAnomaly(M, orbit.eccentricity);
            
            v = Math.Atan2(Math.Sqrt(1-Math.Pow(orbit.eccentricity,2)) * Math.Sin(EA), Math.Cos(EA) - orbit.eccentricity);
        }

        return v;
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

    // Checks what SOI a location is currently in and returns the corresponding cBody
    public static (CelestialBody, Vector3) GetSOI(CartesianData location)
    {
        if (PlanetSystem.Instance != null)
        {
            // Set SOI to infinity if orbit doesn't exist (only applicable to root body) 
            double currentPlanetSOI = location.parent.orbit == null ? double.PositiveInfinity : location.parent.orbit.sphereOfInfluence;

            if (location.parent != null)
            {
                if (location.position.DistanceTo(Vector3.Zero) <= currentPlanetSOI)
                {
                    // Search orbiting bodies
                    foreach (CelestialBody cBody in location.parent.childPlanets)
                    {
                        double cBodySOI = cBody.orbit == null ? double.PositiveInfinity : cBody.orbit.sphereOfInfluence;
                        // As part of the large world coordinate refactor, this weird inconsistent 
                        // coordinate system should be removed. For now, a stupid workaround.
                        // Convert to double3 to use its weird coordinate switching function and back.
                        // I hate this. -R
                        // GetPosYUp should be eliminated... soon.
                        if (location.position.DistanceTo(GetPosYUp(cBody.cartesianData.position)) < cBodySOI)
                        {
                            return (cBody, location.position - GetPosYUp(cBody.cartesianData.position));
                        }
                    }
                    // Return current cBody because we are not within any child SOI
                    return (location.parent, location.position);
                }else{
                    // Return parent body because we are outside the sphere of influence
                    return (location.parent.orbit.parent, location.position + GetPosYUp(location.parent.cartesianData.position));
                }
            }else{
                // Return root body as last resort fallback
                // This should NEVER run because the outputted position is ambiguous!
                GD.Print("Uh oh");
                return (PlanetSystem.Instance.rootBody, Vector3.Zero);
            }
        }else{
            // No planets exist so we can't return anything
            GD.Print("PlanetSystem Instance has not been set! It literally doesn't exist what are you doing!?!");
            // Vector3 is not nullable, but NaNs are possible.
            return (null, new Vector3(double.NaN, double.NaN, double.NaN));
        }
    }
}