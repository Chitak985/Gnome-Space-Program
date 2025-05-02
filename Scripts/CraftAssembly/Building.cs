using Godot;
using System;
using System.Threading.Tasks;

public partial class Building : Area3D
{
	[Export] private MeshInstance3D mesh;
	[Export] private Material hoverGlow;
	[Export] private Control tooltipContainer;
	[Export] private PackedScene tooltip;
	[Export] private string targetScene;
	[Export] private string tooltipText;

	private bool inside;

	private Label buildingTip;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(IsInstanceValid(buildingTip))buildingTip.Position = GetWindow().GetMousePosition();
	}

	public void OnMouseEntered()
	{
		inside = true;
		GD.Print("Mouse entered!");
		mesh.MaterialOverlay = hoverGlow;
		
		buildingTip = (Label)tooltip.Instantiate();
		//buildingTip.Text = tooltipText;
		tooltipContainer.AddChild(buildingTip);

		Tween fontTween = CreateTween();
		fontTween.TweenProperty(buildingTip, "modulate", new Color(1,1,1,1), 0.2f);

		TypingEffect(tooltipText, buildingTip, 10, 100);
	}

	public void OnMouseExited()
	{
		inside = false;
		GD.Print("Mouse exited!");
		mesh.MaterialOverlay = null;
		if(IsInstanceValid(buildingTip))buildingTip.QueueFree();
	}

	private async void TypingEffect(string textToType, Label label, int delay, int waitTime)
	{
		string text = "";
		await Task.Delay(waitTime);
		for (int i = 0; i < textToType.Length; i++)
		{
			await Task.Delay(delay);
			text += textToType[i];
			if(IsInstanceValid(label))
			{
				label.Text = text;
			}else{
				break;
			}
		}
	}

    public void OnInputEvent(Camera3D cam, InputEvent @event, Vector3 eventPos, Vector3 normal, int shapeIDX)
    {
        if (@event is InputEventMouseButton mouse && mouse.Pressed)
		{
			if (Input.IsMouseButtonPressed(MouseButton.Left))
			{
				GetTree().ChangeSceneToFile(targetScene);
			}
		}
    }
}
