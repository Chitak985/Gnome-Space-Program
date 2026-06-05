using Godot;
using System;

public partial class MapIcon : Control
{
    [Export] private Button button;
    [Export] private TextureRect icon;
    [Export] private RichTextLabel flavourText;

    [Export] private float effectDuration = 0.5f;
    [Export] private float textMaxLength = 420;
    [Export] private float iconRotation = 360;

    // Actual stuff
    public MapObject Target { get; private set; }

    public override void _Ready()
    {
        OnMouseExit();
    }

    public override void _Process(double delta)
    {
        ProcessPosition();
    }

    private void ProcessPosition()
    {
        Vector3 globalPos = Target.GlobalPosition;

        Camera3D cam = (Camera3D)MapView.Instance.mapCamera.CamNode; // Just trust me on this one okay

        if (!cam.IsPositionBehind(globalPos))
        {
            Vector2 UIPos = cam.UnprojectPosition(globalPos);
            Position = UIPos - Size / 2;
        }
    }

    public void Initialize(MapObject target)
    {
        Target = target;
    }

    private void OnMouseClick()
    {
        MapView.Instance.mapCamera.TargetObject(Target, 10, 1, 100000);
    }

    private void OnMouseEnter()
    {
        ShowFlavourText(true);
        RotateIcon(true);
    }

    private void OnMouseExit()
    {
        ShowFlavourText(false);
        RotateIcon(false);
    }

    private void ShowFlavourText(bool toggle)
    {
        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Expo);
        tween.SetEase(Tween.EaseType.Out);
        if (toggle)
        {
		    tween.TweenProperty(flavourText, "size", new Vector2(textMaxLength, flavourText.Size.Y), effectDuration);
        }else{
		    tween.TweenProperty(flavourText, "size", new Vector2(0, flavourText.Size.Y), effectDuration);
        }
    }

    private void RotateIcon(bool toggle)
    {
        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Expo);
        tween.SetEase(Tween.EaseType.Out);
        if (toggle)
        {
		    tween.TweenProperty(icon, "rotation_degrees", iconRotation, effectDuration);
        }else{
		    tween.TweenProperty(icon, "rotation_degrees", 0, effectDuration);
        }
    }
}
