using Godot;
using Godot.Collections;
using System.Collections.Generic;

/*
    This object is a little bit baffling so here's my best attempt at explaining it:

    
*/
public partial class Craft : Node3D
{
    public Dictionary partData;
    public Part centralPart; // The absolute root of the craft, what we orient around
    public List<Part> loadedParts = [];

    // Hiujjj??
    public void Instantiate()
    {
        Instantiate(partData);
    }

    // I don't wants parts to be individually simulated so we mangle the shit out of this physics engine
    // Just kidding !
    public void Instantiate(Dictionary partData)
    {
        RealityTangler.Instance.OriginReset += ResetOrigin;
        this.partData = partData;
        foreach (KeyValuePair<Variant, Variant> data in partData)
        {
            // we realize what we're looking at is a part ID (at least we hope)
            if (data.Key.VariantType == Variant.Type.Int)
            {
                string partName = (string)((Dictionary)data.Value)["name"];
                Dictionary theActualEffingData = (Dictionary)((Dictionary)data.Value)["data"];

                CachedPart cachedPart = PartManager.Instance.partCache[partName];

                Part part = cachedPart.Instantiate(this, false);
                part.ReadData(theActualEffingData);
                part.TopLevel = true;
                loadedParts.Add(part);
            }
        }
    }

    public void SetPartPosition()
    {
        SetPartPosition(GlobalPosition);
    }

    public void SetPartPosition(Vector3 position)
    {
        
        foreach (Part part in loadedParts)
        {
            
        }
    }

    // Middle-man function in case we want something special to happen
    public void SnatchFocus()
    {
        ActiveSave.Instance.activeThing = this;
        FlightCamera.Instance.TargetObject(this);
    }

    public void ResetOrigin()
    {
        if (ActiveSave.Instance.activeThing == this)
        {
            Logger.Print("RESET");
            GlobalPosition = Vector3.Zero;
        }
    }
}