using Godot;
using System;
using System.Diagnostics;

public partial class OrbitCamera : Node3D
{
	[Export] public bool multiplyScroll;
	[Export] public float lerpSpeed = 1.0f;
	[Export] public float rotationAmnt = 1.0f;

	[Export] public Node3D rotNode_Y;
	[Export] public Node3D rotNode_X;
	[Export] public RemoteTransform3D camera;
	[Export] public Camera3D remoteCam;

	[Export] public Vector3 outMove = new(0,0,1f);

	[Export] public Vector3 outTarget;

	private Vector3 rotTarget_Y;
	private Vector3 rotTarget_X;

	private bool camRotating;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rotTarget_Y = rotNode_Y.RotationDegrees;
		rotTarget_X = rotNode_X.RotationDegrees;
		camera.RemotePath = remoteCam.GetPath();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        float lerpy = lerpSpeed;

		rotNode_Y.RotationDegrees = rotNode_Y.RotationDegrees.Lerp(rotTarget_Y, lerpy);
		rotNode_X.RotationDegrees = rotNode_X.RotationDegrees.Lerp(rotTarget_X, lerpy);
		camera.Position = camera.Position.Lerp(outTarget, lerpy);
	}

    public override void _UnhandledInput(InputEvent @event)
    {
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
						outTarget /= outMove;
					}else{
						outTarget -= outMove;
					}
					break;
				case MouseButton.WheelDown:
					if(multiplyScroll)
					{
						outTarget *= outMove;
					}else{
						outTarget += outMove;
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
}
