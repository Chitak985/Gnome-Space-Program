using Godot;
using System;
using System.Collections.Generic;

// Shares many similarities with ScaledSpace, though serves a completely different function.
public partial class MapView : Node3D
{
    public static MapView Instance { get; private set; }
    public static readonly string classTag = "([color=pink]MapView[color=white])";
    [Export] public float ScaleFactor { get; private set; } = 10000;

    [Export] private Control UIContainer;
    [Export] private StringName openMapEvent;

    [Export] public bool InMap { get; private set; }
    [Export] public bool canEnterMap = true;
    [Export] public MapCamera mapCamera;
    [Export] public MapUI mapUI;

    [Export] private Control mapIconContainer;
    [Export] private PackedScene mapIconPrefab;
    public List<MapIcon> MapIcons { get; private set; } = [];

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
                Node3D camObject = mapCamera.target;
                Vector3 focusObjectPos = Vector3.Zero;

                if (camObject is MapObject mapObj)
                {
                    focusObjectPos = mapObj.truePosition;

                    if (mapObj.counterpart is Colony colonyObj)
                    {
                        focusObjectPos = colonyObj.parentBody.mapObject.truePosition;
                    }
                }

                mapObject.GlobalPosition = mapObject.truePosition / ScaleFactor - (focusObjectPos / ScaleFactor);
                mapObject.Scale = mapObject.originalScale / ScaleFactor;
            }
        }
    }

    public void ToggleMap(bool toggle)
    {
        InMap = toggle;
        UIContainer.Visible = toggle;

        Logger.Print($"{classTag} Toggled map view to {toggle}");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Open map and only open if active thing isn't null (otherwise don't allow it)
        if (@event.IsActionPressed(openMapEvent))
        {
            ToggleMap(!InMap);
        }
    }

    public void AddMapIcon(MapObject mapObject, Image icon = null)
    {
        MapIcon mapIcon = mapIconPrefab.Instantiate<MapIcon>();
        mapIconContainer.AddChild(mapIcon);
        mapIcon.Initialize(mapObject);

        MapIcons.Add(mapIcon);
    }
}
