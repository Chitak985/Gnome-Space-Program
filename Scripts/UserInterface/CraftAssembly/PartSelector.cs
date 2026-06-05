using Godot;
using System;

public partial class PartSelector : Button
{
    [Export] public SubViewportContainer vpContainer;
    [Export] public Camera3D vpCam;
    [Export] public Node3D partContainer;
    [Export] public float zoomCompMult = 0.8f;
    [Export] public float sizeUp = 1.2f;
    [Export] public float sizeUpDuration = 0.2f;
    [Export] public Vector3 originalRotation = new(-20,45,0);
    [Export] public Vector3 rotationDirection = new(1,1,1);
    [Export] public float rotationSpeed = 0.1f;

    public Control partPreviewContainer;
    public CachedPart partRef;
    public bool partRotating;

    public override void _Ready()
    {
        MouseEntered += () => ExpandPart(true);
        MouseExited += () => ExpandPart(false);

        Pressed += OnPress;
    }

    public override void _Process(double delta)
    {
        //Node3D camPivot = (Node3D)vpCam.GetParent();
        if (partRotating)
        {
            partContainer.RotationDegrees = partContainer.RotationDegrees.Lerp(partContainer.RotationDegrees + rotationDirection, rotationSpeed * (float)delta);
        }else{
            partContainer.RotationDegrees = partContainer.RotationDegrees.Lerp(originalRotation, rotationSpeed / 5f * (float)delta);
        }
    }

    // Loads a passive version of a part into itself and assigns all the important values
    public void LoadPart()
    {
        Part partObj = partRef.Instantiate(partContainer, anchored:true);
        partObj.enabled = false;

        partObj.GlobalPosition = Vector3.Zero;

        //vpCam.LookAt(partObj.CenterOfMass + partObj.GlobalPosition);

        vpCam.Position *= partObj.GetAABB().Size.Length() * zoomCompMult;
    }

    public void ExpandPart(bool expand)
    {
        Vector2 targetSize = CustomMinimumSize;
        Vector2 targetPos = Vector2.Zero;
        vpContainer.ZIndex = 0;
        partRotating = expand;

        if (expand)
        {
            targetSize = CustomMinimumSize * sizeUp;

            targetPos = -Vector2.One * ((targetSize.X - CustomMinimumSize.X) / 2.0f);

            vpContainer.ZIndex = 100;

            vpContainer.GetParent().RemoveChild(vpContainer);
            partPreviewContainer.AddChild(vpContainer);
            vpContainer.Size = CustomMinimumSize;
            vpContainer.GlobalPosition = GlobalPosition;
        }else{
            // Yeah uhhh hmm yeah huh
            Vector2 tempSize = CustomMinimumSize * sizeUp;
            Vector2 tempPos = -Vector2.One * ((tempSize.X - CustomMinimumSize.X) / 2.0f);

            vpContainer.GetParent().RemoveChild(vpContainer);
            AddChild(vpContainer);
            vpContainer.Size = tempSize;
            vpContainer.GlobalPosition = GlobalPosition + tempPos;
        }

        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Expo);
        tween.SetEase(Tween.EaseType.Out);
        tween.SetParallel(true);

        tween.TweenProperty(vpContainer, "size", targetSize, sizeUpDuration);
        tween.TweenProperty(vpContainer, "global_position", GlobalPosition + targetPos, sizeUpDuration);
    }

    // Instantiate a "fake" part

    // The VAB in BuildingManager has to be set for this to work. 
    // You shouldn't use this class at all for cases such as dynamic craft editing.
    public void OnPress()
    {
        BuildingManager.Instance.InsertPart(partRef);
    }
}
