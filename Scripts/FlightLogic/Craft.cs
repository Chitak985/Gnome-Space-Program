using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*
    This object is a little bit baffling so here's my best attempt at explaining it:

    🤷
*/
public partial class Craft : Node3D
{
    public Dictionary PartData { get; private set; }
    public Part CentralPart { get; private set; } // The absolute root of the craft, what we orient around
    public List<Part> LoadedParts { get; private set; } = [];

    // Orbits and positions are ALWAYS in global (non-rotating) space.
    public OrbitDriver OrbitDriver { get; private set; }

    // If the craft is physically loaded (This will affect how its positioning works!)
    public bool Loaded { get; private set; }
    // Whether to lock the craft's physics
    public bool Anchored { get; private set; }
    public MapObject MapObject { get; private set; }
    public OrbitRenderer OrbitRenderer { get; private set; }

    public void Init(Dictionary partData)
    {
        // Connect signals
        RealityTangler.Instance.OrbitProcess += UpdateOrbit;
        RealityTangler.Instance.OrbitProcess += UpdateMap;
        ActiveSave.Instance.TimeLevelChanged += OnTimeLevelChanged;
        ActiveSave.Instance.TimeLevelSafeState += OnTimeLevelSafe;

        PartData = partData;

        // Add map object
        MapObject = MapView.Instance.AddMapObject(Name);
        MapView.Instance.AddMapIcon(MapObject);

        Instantiate();

        Anchor(true);
    }

    public void Instantiate()
    {
        RealityTangler.Instance.OriginReset += ResetOrigin;
        AddPartFromData(PartData, parentObject: this);

        CentralPart = LoadedParts[0];
    }

    public void SetOrbitDriver(OrbitDriver driver)
    {
        // Delete old driver
        OrbitDriver?.QueueFree();

        OrbitDriver = driver;
    }

    public void UpdateMap()
    {
        if (MapObject != null && OrbitDriver != null)
        {
            MapObject.truePosition = OrbitDriver.cartesian.position + OrbitDriver.parent.GlobalCartesianPosition;
        }
    }

    public void UpdateOrbit()
    {
        if (OrbitDriver != null)
        {
            if (OrbitDriver.enabled)
            {
                // Copy root part's state to the driver's cartesian info if we're not on rails
                if (!OrbitDriver.OnRails)
                {
                    // Because cartesian position is relative to planet
                    Vector3 relativePos = CentralPart.GlobalPosition - OrbitDriver.parent.GlobalPosition;

                    Vector3 finalPos = relativePos;
                    Vector3 finalRot = CentralPart.Rotation;
                    Vector3 finalVel = CentralPart.LinearVelocity;

                    if (RealityTangler.Instance.activeReferenceFrame != null)
                    {
                        CelestialBody reference = RealityTangler.Instance.activeReferenceFrame;
                        finalPos = reference.GetGlobalPositionOfPoint(relativePos);
                        finalVel = reference.GetGlobalVelocity(CentralPart.LinearVelocity + reference.GetSurfaceRotationVelocity(finalPos));
                    }

                    OrbitDriver.cartesian.position = finalPos;
                    OrbitDriver.cartesian.rotation = finalRot;
                    OrbitDriver.cartesian.velocity = finalVel; // Feels sloppy but all we can do is pray
                }else{
                    Vector3 finalPos = OrbitDriver.cartesian.position;

                    if (RealityTangler.Instance.activeReferenceFrame != null)
                    {
                        CelestialBody reference = RealityTangler.Instance.activeReferenceFrame;
                        finalPos = reference.GetLocalPositionOfPoint(finalPos);
                    }

                    GlobalPosition = finalPos;
                    FixCraft();
                }

                OrbitDriver.Update();

                if(!Anchored) GlobalPosition = CentralPart.GlobalPosition;

                // UHHH FUCK
                double altitude = OrbitDriver.parent.radius - OrbitDriver.cartesian.position.DistanceTo(Vector3.Zero);

                if (altitude > OrbitDriver.parent.inverseRotAltitude)
                {
                    if (RealityTangler.Instance.activeReferenceFrame != null) RealityTangler.Instance.SwitchReferenceFrame(null);
                }else{
                    if (RealityTangler.Instance.activeReferenceFrame != OrbitDriver.parent)
                    {
                        RealityTangler.Instance.SwitchReferenceFrame(OrbitDriver.parent);
                        SetTransformFromCartesian();
                    } 
                }
            }
        }
    }

    public void Anchor(bool toggle)
    {
        Anchored = toggle;
        foreach (Part part in LoadedParts)
        {
            part.Anchor(toggle);
        }
    }

    // Recursive function to reconstruct a bunch of parts from given part data
    public void AddPartFromData(Dictionary data, Node3D parentObject = null, Part parentPart = null)
    {
        // Assign parent object to the part if it's null
        parentObject ??= parentPart;

        // Instantiate from name
        string partName = (string)data["name"];
        CachedPart cachedPart = PartManager.Instance.partCache[partName];
        Part part = cachedPart.Instantiate(parentObject);
        part.parentThing = this;
        part.Anchor(true); // Anchor it for now
        part.cachedPart = cachedPart;

        if (parentPart != null)
        {
            // Handle attachments (pray that nothing goes wrong at this step)
            int parentNodeIndex = (int)data["parentNode"];
            part.parentNode = parentPart.attachNodes[parentNodeIndex];
            int usedNodeIndex = (int)data["usedNode"];
            part.usedNode = part.attachNodes[usedNodeIndex];

            // Adjust transform to be relative to attachment (NO ROTATION TRANSFORM - PLEASE IMPLEMENT SOON AND MAKE IT NOT ASS)
            if (part.parentNode != null)
            {
                part.Position = part.parentNode.Position - part.usedNode.Position;

                part.CreateAttachJoints(part.usedNode, part.parentNode);
            }

            // Assign parent
            part.parentPart = parentPart;
        }
        
        // ADD HANDLING FOR MODULES

        // Add the part to the list before moving on to its attachments
        LoadedParts.Add(part);

        Godot.Collections.Array attachedParts = (Godot.Collections.Array)data["attachedParts"];
        // loop over attached parts
        foreach (Dictionary childData in attachedParts.Select(v => (Dictionary)v))
        {
            AddPartFromData(childData, parentPart: part);
        }
    }

    // Middle-man function in case we want something special to happen
    public void SnatchFocus()
    {
        StateManager.Instance.ChangeGameState(StateManager.GameState.Flight);
        StateManager.Instance.ChangeFlightState(new StateManager.FlightState() {activeCraft = this});
        FlightCamera.Instance.TargetObject(this, 25, 1, 10000);

        //RealityTangler.Instance.SwitchReferenceFrame();
    }

    public void ResetOrigin()
    {
        if (StateManager.Instance.CurrentFlightState.activeCraft == this)
        {
            Logger.Print("RESET");
            GlobalPosition = Vector3.Zero;
        }
    }

    // Returns all parts to their original position
    public void FixCraft()
    {
        foreach (Part part in LoadedParts)
        {
            if (part.parentPart != null)
            {
                part.Position = part.parentNode.Position - part.usedNode.Position;
            }else{
                part.Position = Vector3.Zero;
            }
        }
    }

    // Sets the velocity from the cartesian data
    public void SetVelocityFromCartesian()
    {
        foreach (Part part in LoadedParts)
        {
            Vector3 finalVel = OrbitDriver.cartesian.velocity;

            // Subtract planet's rotation if we're in a geocentric reference frame
            if (RealityTangler.Instance.activeReferenceFrame != null)
            {
                CelestialBody activeFrame = RealityTangler.Instance.activeReferenceFrame;
                finalVel -= activeFrame.GetSurfaceRotationVelocity(OrbitDriver.cartesian.position);
                finalVel = activeFrame.GetLocalVelocity(finalVel);
            }

            part.LinearVelocity = finalVel;
        }
    }

    // For easy use with the physics sim, it's safe to set the position of the node if the craft is anchored.
    public void SetTransformFromCartesian(bool returnVelocity = true)
    {
        Anchor(true);

        Vector3 positionResult = OrbitDriver.cartesian.position;
        Vector3 rotationResult = OrbitDriver.cartesian.rotation;

        if (RealityTangler.Instance.activeReferenceFrame != null)
        {
            CelestialBody activeReference = RealityTangler.Instance.activeReferenceFrame;
            positionResult = activeReference.GetLocalPositionOfPoint(OrbitDriver.cartesian.position);
        }

        Position = positionResult;
        // Let Godot do the rotating for us
        GlobalRotation = rotationResult;

        foreach (Part part in LoadedParts)
        {
            if (part.parentPart != null)
            {
                part.Position = part.parentNode.Position - part.usedNode.Position;
            }else{
                part.Position = Vector3.Zero;
            }
        }

        Anchor(false);
        
        // Return the velocity lost from anchoring
        if(returnVelocity) SetVelocityFromCartesian();
    }

    public Aabb GetAABB()
    {
        Aabb aabb = new();
        foreach (Part part in LoadedParts)
        {
            aabb = aabb.Merge(part.GetAABB());
        }
        return aabb;
    }
    private void ToggleOnRailsOrbit(bool toggle)
    {
        Logger.Print($"Setting craft ({this}) on-rails orbit to ({toggle})");

        Anchor(toggle);
        OrbitDriver.ToggleOnRailsOrbit(toggle);
        
        // Return to every value we had previously
        if (toggle)
        {
            OrbitDriver.InitCraftPropagator();
        }else{
            SetTransformFromCartesian();
        }
    }

    private void OnTimeLevelSafe()
    {
        Task.Delay(TimeSpan.FromSeconds(CraftManager.Instance.TimeEaseDuration)).ContinueWith(_ =>
        {
            if (OrbitDriver.OnRails) CallDeferred(nameof(ToggleOnRailsOrbit), false);
        });
    }

    private void OnTimeLevelChanged(int newTime)
    {
        if (newTime > ActiveSave.Instance.maxPhysicsSpeedLevel)
        {
            if (!OrbitDriver.OnRails) ToggleOnRailsOrbit(true);
        }
    }
}