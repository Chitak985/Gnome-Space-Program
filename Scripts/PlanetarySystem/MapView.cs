using Godot;
using System;

// Shares many similarities with ScaledSpace, though serves a completely different function.
public partial class MapView : Node3D
{
    public static MapView Instance { get; private set; }
    [Export] public float scaleFactor = 10000;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        Godot.Collections.Array<Node> childNodes = GetChildren();
        foreach (Node node in childNodes)
        {
            if (node is MapObject mapObject)
            {
                Node3D camObject = FlightCamera.Instance.target;
                Vector3 focusObjectPos = Vector3.Zero;

                if (camObject is MapObject mapObj)
                {
                    focusObjectPos = mapObj.truePosition;

                    if (mapObj.counterpart is Colony colonyObj)
                    {
                        
                        focusObjectPos = colonyObj.parentBody.mapObject.truePosition;
                    }
                }

                mapObject.GlobalPosition = mapObject.truePosition / scaleFactor - (focusObjectPos / scaleFactor);
                mapObject.Scale = mapObject.originalScale / scaleFactor;
            }
        }
    }
}
