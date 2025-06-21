using Godot;
using System;
using System.Collections.Generic;

public partial class Craft : RigidBody3D
{
    // Probably don't use this
    public Double3 truePosition;

    [Export] public Node modules;

    [Export] public CelestialBody currentInfluence;

    public double mass;

    public OrbitDriver orbitDriver;

    // Debug tool similar to freecam, freely moves craft around independent of patched conics or physics
    [Export] public bool freeMovement;
    [Export] public float DEBUG_moveSpeed;
    public Vector3 DEBUG_freeMotion;
    public bool DEBUG_rotating;

    public override void _Ready()
    {
        // Set influence to root body (This will need to be changed to whatever body this craft is being "launched" from!)
        currentInfluence = PlanetSystem.Instance.rootBody;

        // Initialize craft modules
        foreach (Node node in modules.GetChildren())
        {
            if (node is OrbitDriver driver)
            {
                driver.craft = this;
                orbitDriver = (OrbitDriver)node;
                orbitDriver.Initialize();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        truePosition = Double3.ConvertToDouble3(Position);

        if (freeMovement)
        {
            Freeze = true;

            Translate(DEBUG_freeMotion);

            if (Input.IsKeyPressed(Key.W)){
                DEBUG_freeMotion.Z = -DEBUG_moveSpeed;
            }else if (Input.IsKeyPressed(Key.S)){
                DEBUG_freeMotion.Z = DEBUG_moveSpeed;
            }else{
                DEBUG_freeMotion.Z = 0;
            }

            if (Input.IsKeyPressed(Key.A)){
                DEBUG_freeMotion.X = -DEBUG_moveSpeed;
            }else if (Input.IsKeyPressed(Key.D)){
                DEBUG_freeMotion.X = DEBUG_moveSpeed;
            }else{
                DEBUG_freeMotion.X = 0;
            }

            if (Input.IsKeyPressed(Key.R)){
                DEBUG_freeMotion.Y = DEBUG_moveSpeed;
            }else if (Input.IsKeyPressed(Key.F)){
                DEBUG_freeMotion.Y = -DEBUG_moveSpeed;
            }else{
                DEBUG_freeMotion.Y = 0;
            }

            if (Input.IsKeyPressed(Key.E)){
                RotateObjectLocal(Vector3.Forward, 0.02f);
            }else if (Input.IsKeyPressed(Key.Q)){
                RotateObjectLocal(Vector3.Forward, -0.02f);
            }
        }else{
            Freeze = false;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Debug free motion stuff
        if (freeMovement)
        {
            if (@event is InputEventMouseButton buttonEvent && buttonEvent.Pressed)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    DEBUG_rotating = true;
                }
            }else if (@event is InputEventMouseButton buttonEvent2 && buttonEvent2.Pressed == false){
                DEBUG_rotating = false;
            }

            if (@event is InputEventMouseMotion motion && DEBUG_rotating == true)
            {
                RotateObjectLocal(Vector3.Up, -motion.Relative.X/300);
                RotateObjectLocal(Vector3.Right, -motion.Relative.Y/300);
            }

            if (Input.IsMouseButtonPressed(MouseButton.WheelUp))
            {
                DEBUG_moveSpeed *= 1.1f;
            }
            if (Input.IsMouseButtonPressed(MouseButton.WheelDown))
            {
                DEBUG_moveSpeed /= 1.1f;
            }
        }
    }
}