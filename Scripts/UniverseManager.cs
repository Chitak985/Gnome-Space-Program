using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO: make a "sphre of influence" system to keep the influencing planet at 0,0,0 (floating point imprecision bullshit)
// Somehow invert the position of every body so that the actual planet of influence remains stationary??? HOW TF DO I DO THAT?

public partial class UniverseManager : Node
{
	[Export] public bool running = true;
	[Export] public bool playerAffectedByOrigin = false;
	[Export] public Node3D player;
	[Export] public Node3D cameraPivot;
	[Export] public float maxDistanceFromOrigin = 100;
	[Export] public float scaleDownFactor = 10.0f;
	//[Export] public float distanceFalloff = 10.0f;
	//[Export] public float scaleFalloff = 10.0f;

	[Export] public float moveForward = 1f;

	[Export] public Planet currentPlanet;

	public Camera3D currentCamera;
	
	public Vector3 offsetPosition;
	public List<ScaledObject> objectList = new();

	private Node3D localSpace;
	private Node3D scaledSpace;

	private Vector3 scaledPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		localSpace = (Node3D)GetTree().GetFirstNodeInGroup("LocalSpace");
		scaledSpace = (Node3D)GetTree().GetFirstNodeInGroup("ScaledSpace");

		//scaledSpace.Scale = new Vector3(1/scaleDownFactor,1/scaleDownFactor,1/scaleDownFactor);

		if (cameraPivot != null)
		{
			RemoteTransform3D camTransform = new();
			player.GetChild(0).AddChild(camTransform);
			camTransform.RemotePath = cameraPivot.GetPath();
			camTransform.UpdateRotation = false;
		}
	}

    public override void _Process(double delta)
    {
		if (running)
		{
			UpdateScaled();
			cameraPivot?.LookAt(currentPlanet.GlobalPosition, Vector3.Up);
			cameraPivot?.Rotate(Vector3.Right, Mathf.DegToRad(90));
		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// I just changed this whole thing into "Physics Process" on a whim, just to see what happens, and the flickering is GONE!?
		// I can't help but feel angry that a solution is this simple, it must be a bad omen of some sort..
		currentCamera = GetViewport().GetCamera3D();
		//GD.Print(currentCamera);
		if(running)
		{
			Vector3 playerPosition = player.GlobalPosition;
			float playerMagnitude = playerPosition.Length();

			//scaledSpace.GlobalPosition = playerPosition - offsetPosition;

			if (playerMagnitude > maxDistanceFromOrigin)
			{
                if(playerAffectedByOrigin)player.GlobalPosition -= playerPosition;
                //GlobalPosition -= playerPosition;

                // there is a chance for everything to fall apart over time but that ok
                OffsetChildren(localSpace, playerPosition);
               // OffsetChildren(scaledSpace, playerPosition);
			   	scaledSpace.GlobalPosition -= playerPosition;
	
			
				offsetPosition -= playerPosition;
			}
		}
	}

	public static void OffsetChildren(Node3D parentNode, Vector3 offset)
	{
		foreach (Node3D goon in parentNode.GetChildren().Cast<Node3D>())
		{
			goon.GlobalPosition -= offset;
		}
	}

	public void UpdateScaled()
	{
		Vector3 plrAdjustedPosition = currentCamera.GlobalPosition - offsetPosition;
		foreach (ScaledObject obj in objectList)
		{
			float magnitude = (obj.truePosition - plrAdjustedPosition).Length();

			Vector3 direction = obj.truePosition.DirectionTo(plrAdjustedPosition);

			float targetScale = 1/scaleDownFactor;

			obj.associatedNode.Position = obj.truePosition+direction*(magnitude/(1+(moveForward/1000f))); // why the fuck is it 1.0101?
			obj.associatedNode.Scale = new Vector3(targetScale,targetScale,targetScale);
		}
	}
}
