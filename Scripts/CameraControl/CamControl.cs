using Godot;
using System;

public partial class CamControl : Node3D
{
    [Export] public Node3D target;
    [Export] public Node3D ground;

    // Motion parameters
    [Export] public bool multiplyScroll;
    [Export] public float lerpSpeed = 1.0f;
    [Export] public float rotationAmnt = 1.0f;
    [Export] public float zoomAmnt;

    // Zoom info
    [Export] public bool canZoom = true;
    [Export] public float zoom;
    [Export] public float minZoom;
    [Export] public float maxZoom;

    [Export] private Node3D rotNode_Y;
    [Export] private Node3D rotNode_X;
    [Export] public Node3D CamNode { get; private set; }

    // Input mapping
    [Export] private StringName dragCam;
    [Export] private StringName zoomIn;
    [Export] private StringName zoomOut;

    private Vector3 rotTargetX;
    private Vector3 rotTargetY;

    private bool camRotating;

    public void Update()
    {
        float lerpy = lerpSpeed;

        rotNode_Y.RotationDegrees = rotNode_Y.RotationDegrees.Lerp(rotTargetY, lerpy);
        rotNode_X.RotationDegrees = rotNode_X.RotationDegrees.Lerp(rotTargetX, lerpy);

        CamNode.Position = CamNode.Position.Lerp(new Vector3(0,0,zoom), lerpy);

        if (target != null) Position = target.GlobalPosition;

        if (ground != null)
        {
            LookAt(ground.GlobalPosition, Vector3.Up);
            //Rotate(Vector3.Right, Math.PI / 2);
        }else{
            GlobalRotation = new Vector3(0, 0, 0);
        }
    }

    public void TargetObject(Node3D target, float zoom, float minZoom, float maxZoom)
    {
        this.target = target;
        this.zoom = zoom;
        this.minZoom = minZoom;
        this.maxZoom = maxZoom;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(dragCam))
        {
            camRotating = true;
        }

        if (@event.IsActionReleased(dragCam))
        {
            camRotating = false;
        }
        
        if (@event.IsAction(zoomIn))
        {
            // Zoom in
            if(multiplyScroll)
            {
                if(canZoom) zoom /= zoomAmnt;
            }else{
                if(canZoom) zoom -= zoomAmnt;
            }
        }else if (@event.IsAction(zoomOut))
        {
            // Zoom out
            if(multiplyScroll)
            {
                if(canZoom) zoom *= zoomAmnt;
            }else{
                if(canZoom) zoom += zoomAmnt;
            }
        }
    

        if (@event is InputEventMouseMotion motion && camRotating == true)
        {
            rotTargetY += Vector3.Up * -motion.Relative.X*rotationAmnt;
            rotTargetX += Vector3.Right * -motion.Relative.Y*rotationAmnt;
        }
    }
}
