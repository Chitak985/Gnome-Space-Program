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
    
    public double Throttle { get; private set; }

    public void UpdateOrbit()
    {
        if (OrbitDriver != null)
        {
            OrbitDriver.Update();

            // Copy root part's state to the driver's cartesian info if we're not on rails
            if (!OrbitDriver.OnRails)
            {
                // Because cartesian position is relative to planet
                Vector3 relativePos = CentralPart.GlobalPosition - OrbitDriver.ParentCBody.GlobalPosition;

                Vector3 finalPos = relativePos;
                Vector3 finalRot = CentralPart.GlobalRotation;
                Vector3 finalVel = CentralPart.LinearVelocity;

                if (RealityTangler.Instance.RotatingReferenceFrame != null)
                {
                    CelestialBody reference = RealityTangler.Instance.RotatingReferenceFrame;
                    finalPos = reference.GetAbsolutePositionOfPoint(relativePos);
                    finalVel = reference.GetAbsoluteVelocity(CentralPart.LinearVelocity) + reference.GetSurfaceRotationVelocity(finalPos);
                }

                OrbitDriver.CartState.elements.position = finalPos;
                OrbitDriver.CartState.elements.rotation = finalRot;
                OrbitDriver.CartState.elements.velocity = finalVel; // Feels sloppy but all we can do is pray
            }else{
                // Propagation is done in the orbit driver
                Vector3 finalPos = OrbitDriver.CartState.elements.position;

                if (RealityTangler.Instance.RotatingReferenceFrame != null)
                {
                    CelestialBody reference = RealityTangler.Instance.RotatingReferenceFrame;
                    finalPos = reference.GetLocalPositionOfPoint(finalPos);
                }

                GlobalPosition = finalPos;

                FixCraft();
            }

            if (!Anchored)
            {
                GlobalPosition = CentralPart.GlobalPosition;
            }
        }
    }

    public void Init(Dictionary partData, CelestialBody parentCBody)
    {
        CreateOrbitDriver(parentCBody);

        OrbitRendererManager.Instance.CreateOrbitRenderer(this);
        
        // Connect signals
        RealityTangler.Instance.OrbitProcess += UpdateOrbit;
        RealityTangler.Instance.OrbitProcess += UpdateMap;
        RealityTangler.Instance.ReferenceFrameChanged += OnReferenceFrameChanged;
        ActiveSave.Instance.TimeLevelChanged += OnTimeLevelChanged;
        ActiveSave.Instance.TimeLevelSafeState += OnTimeLevelSafe;

        PartData = partData;

        // Add map object
        MapObject = MapView.Instance.AddMapObject(Name);
        MapView.Instance.AddMapIcon(MapObject);

        Instantiate();

        Anchor(true);
    }

    // Run this once per craft and never again
    public void CreateOrbitDriver(CelestialBody parentCBody)
    {
        OrbitDriver = new();
        OrbitDriver.ElementsUpdated += OnDriverElementUpdate;
        OrbitDriver.Init(parentCBody, this);
        OrbitDriver.Enabled = true;
    }

    public void Instantiate()
    {
        RealityTangler.Instance.OriginReset += ResetOrigin;
        AddPartFromData(PartData, parentObject: this);

        CentralPart = LoadedParts[0];

        foreach (Part part in LoadedParts)
        {
            part.InitPart();
        }
    }

    public void UpdateMap()
    {
        if (MapObject != null && OrbitDriver != null)
        {
            MapObject.truePosition = OrbitDriver.CartState.elements.position + OrbitDriver.ParentCBody.AbsolutePosition;
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
            Vector3 finalVel = OrbitDriver.CartState.elements.velocity;

            // Subtract planet's rotation if we're in a geocentric reference frame
            if (RealityTangler.Instance.RotatingReferenceFrame != null)
            {
                CelestialBody activeFrame = RealityTangler.Instance.RotatingReferenceFrame;
                finalVel -= activeFrame.GetSurfaceRotationVelocity(OrbitDriver.CartState.elements.position);
                finalVel = activeFrame.GetLocalVelocity(finalVel);
            }

            part.LinearVelocity = finalVel;
        }
    }

    // For easy use with the physics sim, it's safe to set the position of the node if the craft is anchored.
    public void SetTransformFromCartesian(bool returnVelocity = true)
    {
        Anchor(true);

        Vector3 positionResult = OrbitDriver.CartState.elements.position;
        Vector3 rotationResult = OrbitDriver.CartState.elements.rotation;

        if (RealityTangler.Instance.RotatingReferenceFrame != null)
        {
            CelestialBody activeReference = RealityTangler.Instance.RotatingReferenceFrame;
            positionResult = activeReference.GetLocalPositionOfPoint(OrbitDriver.CartState.elements.position);
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
    // Normalize throttle to 0 - 1
    public void SetThrottle(double value)
    {
        if (value > 1)
        {
            Throttle = 1;
        }else if (value < 0) {
            Throttle = 0;
        }else{
            Throttle = value;
        }
    }
    private void ToggleOnRailsOrbit(bool toggle)
    {
        Logger.Print($"Setting craft ({this}) on-rails orbit to ({toggle})");

        Anchor(toggle);
        OrbitDriver.ToggleOnRailsOrbit(toggle, saveMeanAnomaly: true);

        if (!toggle)
            SetTransformFromCartesian();
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
    private void OnReferenceFrameChanged()
    {
        if (!Anchored) SetTransformFromCartesian();
    }
    private void OnDriverElementUpdate()
    {
        SetTransformFromCartesian();
    }
}