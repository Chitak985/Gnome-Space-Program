using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

/*
    This object is a little bit baffling so here's my best attempt at explaining it:

    🤷
*/
public partial class Craft : Node3D
{
    public Dictionary partData;
    public Part centralPart; // The absolute root of the craft, what we orient around
    public List<Part> loadedParts = [];

    public Orbit orbit;
    public CartesianData cartesianData;

    public bool Anchored { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        // Loop over every part and apply a force towards the planet
        foreach (Part part in loadedParts)
        {
            CelestialBody currentCBody = orbit.cBody; // Replace this later PLEAAASEEE AHHH

            Vector3 center = currentCBody.GlobalPosition;
            Vector3 direction = part.GlobalPosition.DirectionTo(center);

            double distance = (center - part.GlobalPosition).Length();
            double planetMass = currentCBody.mass;

            double force = Conics.GravConstant * (planetMass * part.Mass / Mathf.Pow(distance, 2));

            part.ApplyCentralForce(force*direction);
        }
    }

    public void Anchor(bool toggle)
    {
        Anchored = toggle;
        foreach (Part part in loadedParts)
        {
            part.Anchor(toggle);
        }
    }

    // Creates stuff like the orbit and whatnot
    public void Initialize()
    {
        orbit = new();
        cartesianData = new();
    }

    // Hiujjj??
    public void Instantiate()
    {
        Instantiate(partData);
    }

    public void Instantiate(Dictionary partData)
    {
        RealityTangler.Instance.OriginReset += ResetOrigin;
        this.partData = partData;
        AddPartFromData(partData, parentObject: this);

        centralPart = loadedParts[0];
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
        part.Anchor(true); // Anchor it for now
        
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
        }
        
        // ADD HANDLING FOR MODULES

        // Add the part to the list before moving on to its attachments
        loadedParts.Add(part);

        Array attachedParts = (Array)data["attachedParts"];
        // loop over attached parts
        foreach (Dictionary childData in attachedParts.Select(v => (Dictionary)v))
        {
            AddPartFromData(childData, parentPart: part);
        }
    }

    // Middle-man function in case we want something special to happen
    public void SnatchFocus()
    {
        StateManager.Instance.flightState.activeCraft = this;
        FlightCamera.Instance.TargetObject(this);
    }

    public void ResetOrigin()
    {
        if (StateManager.Instance.flightState.activeCraft == this)
        {
            Logger.Print("RESET");
            GlobalPosition = Vector3.Zero;
        }
    }
}