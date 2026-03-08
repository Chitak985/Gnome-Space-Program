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
    public static readonly double Epsilon = 1e-08;
    
    // Functions to get points with Y as up rather than Z
    // To Be Eliminated
    private static Vector3 GetPosYUp(Vector3 inputVector)
    {
        return new Vector3(inputVector.X,inputVector.Z,inputVector.Y);
    }
    
    // Orbital Elements to Cartesian
    public static CartesianData ElemToCart(Orbit orbit)
    {
        // yeah whatever the fRICK
        double MU = orbit.ComputeMU();//GravConstant * parent.mass;

        // Compile our favourite Keplerian orbit elements

        double a = orbit.semiMajorAxis;
        double e = orbit.eccentricity;
        double i = orbit.inclination;
        double omega = orbit.argumentOfPeriapsis;
        double Omega = orbit.longitudeOfAscendingNode;
        double truAN = orbit.trueAnomaly;

        double p = e != 1.0 ? a * (1 - e * e) : 2 * a;
        double r = p / (1 + e * Math.Cos(truAN));

        Vector3 rPQW = new(
            r * Math.Cos(truAN),
            r * Math.Sin(truAN),
            0
        );

        Vector3 vPQW = new(
            -Math.Sqrt(MU / p) * Math.Sin(truAN),
            Math.Sqrt(MU / p) * (e + Math.Cos(truAN)),
            0
        );

        Basis R =
            new Basis(Vector3.Forward, -Omega) * // Rotate around global Z
            new Basis(Vector3.Right, i) *           // Rotate by inclination
            new Basis(Vector3.Forward, -omega);       // Rotate by argument of periapsis

        // Rotate position and velocity vectors
        Vector3 position = R * rPQW;

        Vector3 velocity = R * vPQW;

        return new CartesianData() {
            parent = orbit.parent,
            position = new Vector3(position.X,position.Z,position.Y),
            velocity = new Vector3(velocity.X,velocity.Z,velocity.Y)
        };
    }

    /* Cartesian to Orbital Elements

        Lots of help from https://orbital-mechanics.space/classical-orbital-elements/orbital-elements-and-the-state-vector.html
        And various other pages
    */
   public static Orbit CartToElem(CartesianData data)
    {
        // define mu, vectors
        double mu = GravConstant * data.parent.mass;
        Vector3 rVec = new(data.position.X, data.position.Z, data.position.Y); // Just flip some numbers around and pray
        Vector3 vVec = new(data.velocity.X, data.velocity.Z, data.velocity.Y);
        double r = rVec.Length();
        double v = vVec.Length();

        // Specific angular momentum
        Vector3 hVec = rVec.Cross(vVec);
        double h = hVec.Length();
        double p = h * h / mu;

        // Right Ascension of the Ascending Node
        // Back is (0, 0, 1)
        Vector3 nVec = Vector3.Back.Cross(hVec);
        double N = nVec.Length();

        // Eccentricity
        Vector3 eVec = vVec.Cross(hVec) / mu - rVec / r;
        double e = eVec.Length();

        // Semimajor axis
        double alpha = 2.0 / r - v * v / mu;
        double a;
        if (Math.Abs(alpha) > Epsilon)
        {
            // elliptic or hyperbolic
            a = 1.0 / alpha;
        }else{
            double rp = p / 2;
            a = -rp;
        }

        // Inclination
        double i = Math.Acos(hVec.Z / h);

        // Acending node, argument of periapsis, true anomaly (respectively)
        double Omega = 0;
        double omega = 0;
        double nu = 0;
        if (e >= 1e-11 && i >= 1e-11 && i <= Math.PI - 1e-11)
        {
            // Non circular inclined orbit

            // Ascending node
            Omega = Math.Atan2(nVec.Y, nVec.X);
            if (Omega < 0) Omega += 2 * Math.PI;

            // Argument of periapsis
            omega = Math.Atan2(nVec.Cross(eVec).Dot(Vector3.Back), nVec.Dot(eVec));
            if (hVec.Z < 0) omega = 2 * Math.PI - omega;

            // True anomaly
            nu = Math.Atan2(
                eVec.Cross(rVec).Dot(hVec) / (e * h * r),
                eVec.Dot(rVec) / (e * r)
            );
            if (nu < 0) nu += 2 * Math.PI;
        }else if (e >= 1e-11 && (i < 1e-11 || i > Math.PI - 1e-11))
        {
            // Non circular equatorial orbit

            // Ascending node
            Omega = 0;

            omega = Math.Acos(eVec.X / e);
            // Handle cases where the orbit is retrograde AND FIX THIS TO USE ATAN2
            if (i <= Math.PI - 1e-11)
            {
                if (eVec.Y < 0.0)
                    omega = 2.0 * Math.PI - omega;
            }else{
                if (eVec.Y > 0.0)
                    omega = 2.0 * Math.PI - omega;
            }

            // True anomaly
            nu = Math.Atan2(
                eVec.Cross(rVec).Dot(hVec) / (e * h * r),
                eVec.Dot(rVec) / (e * r)
            );
            if (nu < 0) nu += 2 * Math.PI;
        }else if (e < 1e-11 && i >= 1e-11)
        {
            // Circular inclined orbit

            // Ascending node
            Omega = Math.Atan2(nVec.Y, nVec.X);
            if (Omega < 0) Omega += 2 * Math.PI;

            // Argument of periapsis
            omega = 0;

            // True anomaly
            nu = Math.Atan2(
                nVec.Cross(rVec).Dot(hVec) / (N * h * r),
                nVec.Dot(rVec) / (N * r)
            );
            if (nu < 0) nu += 2 * Math.PI;
        }else if (e < 1e-11 && i < 1e-11)
        {
            // Circular equatorial orbit

            // Ascending node
            Omega = 0;

            // Argument of periapsis
            omega = 0;

            // True anomaly (FIX TO USE ATAN2 I JUTS FORGOT)
            nu = Math.Acos(rVec.X / r);
            if (rVec.Y < 0)
                nu = 2.0 * Math.PI - nu;
        }else{
            Logger.Print("Shit's fucked mate \n (Couldn't determine orbit type)");
        }

        Orbit newOrbit = new()
        {
            parent = data.parent,
            semiMajorAxis = a,
            eccentricity = e,
            inclination = i,
            longitudeOfAscendingNode = Omega,
            argumentOfPeriapsis = omega,
            trueAnomaly = nu,
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
            double currentPlanetSOI = location.parent.OrbitDriver.orbit == null ? double.PositiveInfinity : location.parent.OrbitDriver.orbit.sphereOfInfluence;

            if (location.parent != null)
            {
                if (location.position.DistanceTo(Vector3.Zero) <= currentPlanetSOI)
                {
                    // Search orbiting bodies
                    foreach (CelestialBody cBody in location.parent.childPlanets)
                    {
                        double cBodySOI = cBody.OrbitDriver.orbit == null ? double.PositiveInfinity : cBody.OrbitDriver.orbit.sphereOfInfluence;
                        // As part of the large world coordinate refactor, this weird inconsistent 
                        // coordinate system should be removed. For now, a stupid workaround.
                        // Convert to double3 to use its weird coordinate switching function and back.
                        // I hate this. -R
                        // GetPosYUp should be eliminated... soon.
                        if (location.position.DistanceTo(cBody.OrbitDriver.cartesian.position) < cBodySOI)
                        {
                            return (cBody, location.position - GetPosYUp(cBody.OrbitDriver.cartesian.position));
                        }
                    }
                    // Return current cBody because we are not within any child SOI
                    return (location.parent, location.position);
                }else{
                    // Return parent body because we are outside the sphere of influence
                    return (location.parent.OrbitDriver.orbit.parent, location.position + GetPosYUp(location.parent.OrbitDriver.cartesian.position));
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