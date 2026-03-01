using Godot;
using System;
using System.Collections.Generic;

public partial class CelestialBody : Node3D
{
    // General info
    public string cBodyName;
    public bool focusOnload;
    public double mass;
    public double geeASL;
    public double radius;
    public Vector3 originPos;

    // Rotation info
    public double initialRot;
    public double rot;
    public double rotPeriod;
    public Vector3 tilt;
    // Transform that isn't affected by the body's node transform
    public Transform3D CachedTransform { get; private set; }

    // Axial tilt nodes
    public Node3D pivot;
    public Node3D gimbal;

    // Light info (This might not exist!)
    public CBodyLight light; // This updates independently of the cBody

    // Orbital info
    public string parentName;
    public Orbit orbit;
    public CartesianData cartesianData;

    public List<CelestialBody> childPlanets = [];

    // Procedural info
    public TerrainGen pqsSphere;
    public ScaledObject scaledObject;
    public MapObject mapObject;
    public List<Node> pqsMods;

    // Miscellaneous info
    public bool isRoot; // only ONE body per save should ever have this be true!
    public string configPath;

    // DEBUG
    private Node3D scaledGizmo;
    private Node3D mapGizmo;

    // Deletes old gizmos tooo
    public void CreateGizmo()
    {
        scaledGizmo?.QueueFree();
        scaledGizmo = null;
        mapGizmo?.QueueFree();
        mapGizmo = null;
        
        scaledGizmo = (Node3D)PlanetSystem.Instance.DEBUG_GizmoPrefab.Instantiate();
        scaledGizmo.Scale = Vector3.One * (float)radius * 0.03f;

        foreach (Node node in scaledGizmo.GetChildren())
        {
            if (node is MeshInstance3D mesh)
            {
                mesh.SetLayerMaskValue(1, true);
                mesh.SetLayerMaskValue(2, true);
            }
        }
        scaledObject.AddChild(scaledGizmo);

        mapGizmo = (Node3D)PlanetSystem.Instance.DEBUG_GizmoPrefab.Instantiate();
        mapGizmo.Scale = Vector3.One * (float)radius * 0.03f;
        mapObject.AddChild(mapGizmo);
    }

    public void ToggleGizmo(bool toggle)
    {
        scaledGizmo.Visible = toggle;
        mapGizmo.Visible = toggle;
    }

    public override void _Process(double delta)
    {
        // Propagate the cBody's orbit

        //ProcessOrbitalPosition();

        //scaledSphere.truePosition = GetPosYUp(cartesianData.position);

        //pivot.Rotation += new Vector3(0, 0.01, 0);
    }

    // Process the cBody orbital positioning calculations. Used by RealityTangler to "force" repositioning to avoid jitter.
    public void ProcessTransform()
    {
        // Okay so Godot automatically transforms the rotation and sometimes planets can end up being titled to some baffling degree so just do this and forget
        Rotation = Vector3.Zero;

        if (orbit != null)
        {
            orbit.trueAnomaly = Conics.TimeToTrueAnomaly(orbit, ActiveSave.Instance.SaveTime, 0) + orbit.trueAnomalyAtEpoch;
            CartesianData data = Conics.ElemToCart(orbit);
            cartesianData.position = data.position + orbit.parent.cartesianData.position;
            cartesianData.velocity = data.position;
            //GD.Print(SaveManager.Instance.saveTime);
            //GD.Print($"{cartesianData.position.X}, {cartesianData.position.Y}, {cartesianData.position.Z}");
        }

        // Uh
        if (RealityTangler.Instance.activeReferenceFrame == this)
        {
            originPos = cartesianData.position - RealityTangler.Instance.OriginOffset; 
        }else{
            originPos = cartesianData.position - RealityTangler.Instance.PlanetaryOffset;
        }

        Position = originPos;

        //Logger.Print($"{name} {Position}");

        scaledObject.truePosition = GlobalPosition; //cBody.cartesianData.position.GetPosYUp();

        mapObject.truePosition = cartesianData.position;
        mapObject.Rotation = CachedTransform.Basis.GetEuler();

        // Update rotation
        rot = Math.Tau * (ActiveSave.Instance.SaveTime / rotPeriod); //ActiveSave.Instance.saveTime * rotPeriod;

        Transform3D trans = new()
        {
            Basis = Basis.FromEuler(new Vector3(0, rot, 0))
        };

        // Update cached trash
        Transform3D newCachedTransform = new()
        {
            Basis = pivot.Transform.Basis * trans.Basis
        };
        CachedTransform = newCachedTransform;

        //Logger.Print(this);
        //Logger.Print((PlanetSystem.Instance.localSpacePlanets.Transform.Basis * cachedTransform.Basis).GetEuler());
        //Logger.Print(gimbal.GlobalTransform.Basis.GetEuler());

        // Don't rotate if we're the active reference frame
        if (RealityTangler.Instance.activeReferenceFrame != this)
        {
            gimbal.Transform = trans;
            scaledObject.Rotation = gimbal.GlobalTransform.Basis.GetEuler();
        }else{
            gimbal.GlobalRotation = Vector3.Zero;
            scaledObject.Rotation = Vector3.Zero;
        }
    }

    public void InitializeSelf()
    {
        // Scaled space and map view
        scaledObject = new() { Name = $"{cBodyName}_Scaled" };
        ScaledSpace.Instance.AddChild(scaledObject);

        mapObject = new() { Name = $"{cBodyName}_Map" };
        MapView.Instance.AddChild(mapObject);
        MapView.Instance.AddMapIcon(mapObject);

        // Create gimbals and pivots for axial tilt and all that other jazz
        pivot = new();
        AddChild(pivot);
        pivot.RotationDegrees = tilt;
        pivot.Name = "Pivot";

        gimbal = new();
        pivot.AddChild(gimbal);
        gimbal.Name = "Gimbal";

        pqsSphere = new TerrainGen
        {
            cBody = this,
            runInSeparateThread = false,
            radius = (float)radius,
            Name = "PQS"
        };
        gimbal.AddChild(pqsSphere);

        CreateGizmo();
    }

    public void ResetOrigin()
    {
        // Just to prevent jitter
        ProcessTransform();
    }

    // Gets the global position of a point on the planet (factoring in rotation)
    public Vector3 GetGlobalPositionOfPoint(Vector3 point)
    {
        Transform3D trans = new()
        {
            Origin = point
        };;

        Transform3D finalTrans = CachedTransform * trans;

        return finalTrans.Origin;
    }

    // Returns the velocity vector of the planet's surface at that point IN THE GEOCENTRIC REFERENCE FRAME!!
    // Ate a bit of https://en.wikipedia.org/wiki/Rigid_body_dynamics and shat out this function
    public Vector3 GetSurfaceRotationVelocity(Vector3 point, bool geocentric = false)
    {
        // Rotation period is in seconds per 2pi radians.
        // Angular velocity has to be in radians per second.
        double angularVelocity = Math.Tau / rotPeriod;

        Vector3 planetUp = Vector3.Up;
        // Factor in planet's tilt if it's not geocentric
        if (!geocentric) planetUp = CachedTransform.Basis.Y;

        Vector3 angularVelocityVector = planetUp * angularVelocity;

        Vector3 velocity = angularVelocityVector.Cross(point); // Cross and pray

        return velocity;
    }

    // Shamelessly stolen from https://stackoverflow.com/questions/46247499/vector3-to-latitude-longitude
    // CONVERT POSITION TO GEOCENTRIC REFERENCE FRAME FIRST!
    public Vector2 GetLatitudeLongitude(Vector3 position, bool radians = false)
    {
        double lat = Math.Acos(position.Y / radius); //theta
        double lon = Math.Atan(position.X / position.Z); //phi

        // Skip conversion if we just want radians
        if (radians) return new Vector2(lat, lon);

        double radToDeg = 180 / Math.PI;
        return new Vector2(lat * radToDeg, lon * radToDeg);
    }

    public override string ToString()
    {
        return cBodyName;
    }
}
