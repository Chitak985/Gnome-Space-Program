using Godot;
using System;

/*
Potentially add an "offset" position for when in map view so that scaled space can
move around the camera rather than the camera moving around it 
(also add override when in map view so that the camera can navigate around scaled space -
currently all it is doing is moving in REAL space which causes floating point imprecision bugs)

TL;DR the camera has to look at scaled space in map view, and have scaled space move instead of the camera.
*/
public partial class ScaledSpace : Node3D
{
    // Obligatory.
    public static ScaledSpace Instance { get; private set; }
    // By how much the scale is divided by
    [Export] public float scaleFactor = 10000;
    [Export] public float moveForward = 0.1f;
    [Export] public FlightCamera flightCamera;

    public override void _Ready()
    {
        RealityTangler.Instance.ScaledProcess += ForceUpdate;
        Instance = this;
    }

    // Something secret steers scaled objects to lag behind when this isn't done. Therefore, we do this. I'm sorry God.
    //public override void _Process(double delta)
    //{
     //   if (!flightCamera.inMap) ForceUpdate();
    //}

    public void ForceUpdate()
    {
        Godot.Collections.Array<Node> childNodes = GetChildren();

        foreach (Node node in childNodes)
        {
            if (node is ScaledObject scaledObject)
            {
                // Handle ScaledSpace differently if in map view
                if (!flightCamera.inMap)
                {
                    Vector3 objPos = scaledObject.truePosition;
                    Vector3 camDir = objPos.DirectionTo(flightCamera.camNode.GlobalPosition);

                    double magnitude = (objPos - flightCamera.camNode.GlobalPosition).Length();

                    scaledObject.GlobalPosition = objPos + camDir * (magnitude/(1+(moveForward/1000f)));// + offsetPosition;
                    scaledObject.Scale = scaledObject.originalScale / scaleFactor;
                }else{
                    Node3D camObject = flightCamera.target;
                    Vector3 focusObjectPos = Vector3.Zero;
                    // Check if the camera is focusing on either a ScaledObject or another thing that isn't implemented yet
                    if (camObject is ScaledObject scaledCamObj)
                    {
                        focusObjectPos = scaledCamObj.altPosition; // "alternate" position is an insane hackjob.
                    }
                
                    scaledObject.GlobalPosition = scaledObject.altPosition / scaleFactor - (focusObjectPos / scaleFactor);
                    scaledObject.Scale = scaledObject.originalScale / scaleFactor;
                }
            }
        }
    }
}
