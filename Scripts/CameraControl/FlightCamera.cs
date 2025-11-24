using Godot;
using System;

public partial class FlightCamera : Node3D
{
    // THERE SHOULD ONLY EVER BE ONE FLIGHT CAMERA!!
    // The same camera (THIS ONE) is used in both map view, colony view, and flight
    public static readonly string classTag = "([color=MEDIUM_SPRING_GREEN]FlightCamera[color=white])";
    public static FlightCamera Instance { get; private set; }

    [Export] public bool inMap = true;
    [Export] public bool canEnterMap = true;
    [Export] public Key mapKey = Key.M;
    [Export] public Node3D target;
    public (float, float, float) zoomLimits;
    // local / map dichotomy
    [Export] public Node3D localTarget;
    public (float, float, float) localZoomLimits;
    [Export] public Node3D mapTarget;
    public (float, float, float) mapZoomLimits;

    [Export] public bool multiplyScroll;
    [Export] public float lerpSpeed = 1.0f;
    [Export] public float rotationAmnt = 1.0f;

    [Export] public Node3D rotNode_Y;
    [Export] public Node3D rotNode_X;
    [Export] public Node3D camNode;

    [Export] public float zoomAmnt;
    [Export] public bool canZoom = true;
    [Export] public float zoom;

    [Export] public Node3D facingDownObject;

    private Vector3 rotTarget_Y;
    private Vector3 rotTarget_X;

    private bool camRotating;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        SingletonRegistry.Register(this); // Register self

        rotTarget_Y = rotNode_Y.RotationDegrees;
        rotTarget_X = rotNode_X.RotationDegrees;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Update();
    }

    public void Update()
    {
        // Decide
        target = inMap ? mapTarget : localTarget;
        zoomLimits = inMap ? mapZoomLimits : localZoomLimits;

        float lerpy = lerpSpeed * (float)GetProcessDeltaTime();

        rotNode_Y.RotationDegrees = rotNode_Y.RotationDegrees.Lerp(rotTarget_Y, lerpy);
        rotNode_X.RotationDegrees = rotNode_X.RotationDegrees.Lerp(rotTarget_X, lerpy);

        // Item1 = min Item2 = max Item3 = tgt
        if (zoom > zoomLimits.Item2) zoom = zoomLimits.Item2;
        if (zoom < zoomLimits.Item1) zoom = zoomLimits.Item1;

        camNode.Position = camNode.Position.Lerp(new Vector3(0,0,zoom), lerpy);

        if (target != null) Position = target.GlobalPosition;

        if (facingDownObject != null && !inMap)
        {
            LookAt(facingDownObject.GlobalPosition, Vector3.Up);
            Rotate(Vector3.Right, 1.570795f);
        }else{
            GlobalRotation = new Vector3(0, 0, 0);
        }
    }

    // Map view
    public void ToggleMapView(bool toggle)
    {
        // Leave if we can't do this
        if (toggle && !canEnterMap) return;

        OrbitRendererManager.Instance.ToggleRenderers(toggle);

        MapUI.Instance.Visible = toggle;

        Logger.Print($"{classTag} Map view: {toggle}");
        inMap = toggle;
        ActiveSave.Instance.localSpace.Visible = !toggle;

        zoom = toggle ? mapZoomLimits.Item3 : localZoomLimits.Item3;
        Logger.Print($"{classTag} Zoom is: {zoom}");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Keybinds
        if (@event is InputEventKey keyEvent)
        {
            // Open map and only open if active thing isn't null (otherwise don't allow it)
            if (keyEvent.Keycode == mapKey && keyEvent.Pressed && localTarget != null)
            {
                ToggleMapView(!inMap);
            }
        }

        if (@event is InputEventMouseButton buttonEvent)
        {
            switch (buttonEvent.ButtonIndex)
            {
                case MouseButton.Right:
                    camRotating = buttonEvent.Pressed;
                    break;
                case MouseButton.WheelUp:
                    if(multiplyScroll)
                    {
                        if(canZoom) zoom /= zoomAmnt;
                    }else{
                        if(canZoom) zoom -= zoomAmnt;
                    }
                    break;
                case MouseButton.WheelDown:
                    if(multiplyScroll)
                    {
                        if(canZoom) zoom *= zoomAmnt;
                    }else{
                        if(canZoom) zoom += zoomAmnt;
                    }
                    break;
            }
        }
        if (@event is InputEventMouseMotion motion && camRotating == true)
        {
            rotTarget_Y += Vector3.Up * -motion.Relative.X*rotationAmnt;
            rotTarget_X += Vector3.Right * -motion.Relative.Y*rotationAmnt;
        }
    }

    // We don't know whether this one has a scaled object so we pass an optional parameter
    // Used for map icons. Vector3 limits is for GDscript interop
    public void TargetObject(Node3D node, Vector3 limits, bool mapObj)
    {
        if (!mapObj)
        {
            localTarget = node;
            // cast everything to float... everyone gets a float.
            localZoomLimits = ((float)limits.X,(float)limits.Y,(float)limits.Z);
        }else{
            mapTarget = node;
            mapZoomLimits = ((float)limits.X,(float)limits.Y,(float)limits.Z);
        }
        
        Position = Vector3.Zero;

        Logger.Print($"{classTag} Targeting node: {node.Name}");
        Logger.Print($"{classTag} Node scale: {node.Scale}");

        // Cast to float for compatibility
        zoom = (float)limits.Z; //node.Scale.Z * 2f / ScaledSpace.Instance.scaleFactor;
    }

    // Targeting colonies in LOCAL SPACE
    public void TargetObject(Colony colony)
    {
        localTarget = colony;
        localZoomLimits = (5,10000000,250);
        mapTarget = colony.scaledObject;
        mapZoomLimits = (5,10000000,2);
        Position = Vector3.Zero;

        Logger.Print($"{classTag} Targeting colony: {colony.Name}");

        zoom = 250; //node.Scale.Z * 2f / ScaledSpace.Instance.scaleFactor;
    }

    // Target CRAFT
    public void TargetObject(Craft craft)
    {
        localTarget = craft;
        localZoomLimits = (1,10000000,250);
        //mapTarget = craft.scaledObject;
        mapZoomLimits = (5,10000000,2);
        Position = Vector3.Zero;

        Logger.Print($"{classTag} Targeting craft: {craft.Name}");

        zoom = 5; //node.Scale.Z * 2f / ScaledSpace.Instance.scaleFactor;
    }

    // Planets shouldn't be targeted locally so we don't target the local object
    public void TargetObject(CelestialBody cBody)
    {
        mapTarget = cBody.scaledSphere;
        Position = Vector3.Zero;

        Logger.Print($"{classTag} Targeting cBody: {cBody.Name}");
        Logger.Print($"{classTag} cBody radius: {cBody.radius}");

        mapZoomLimits = (
            (float)(cBody.radius * 1.25f / ScaledSpace.Instance.scaleFactor),
            float.PositiveInfinity,
            (float)(cBody.radius * 2f / ScaledSpace.Instance.scaleFactor));

        zoom = (float)(cBody.radius * 2f / ScaledSpace.Instance.scaleFactor);
    }
}
