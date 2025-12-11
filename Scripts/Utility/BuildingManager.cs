using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class BuildingManager : Node
{
    public static readonly string classTag = "([color=Turquoise]BuildingManager[color=white])";
    public static BuildingManager Instance { get; private set; }

    // Where to instantiate parts
    [Export] public Node3D editorPartContainer;
    [Export] public Node3D floatingPartContainer;
    [Export] public BuildUI buildUI;
    [Export] public float partMoveSpeed = 15f;
    [Export] public float partHoldDistance = 5f;
    [Export] public float partSnapDistance = 30f;
    [Export] public bool verboseLogging;

    public enum EditorMode
    {
        None,
        Static,
        Dynamic
    }

    // None (0), Static (1), or Dynamic (2)
    public EditorMode editorMode = EditorMode.None;
    
    // Guess what, it's the part being dragged!
    public Part draggingPart;
    public (AttachNode, AttachNode) snappedNodes;
    public List<Part> partsList = [];
    // We orient around this one
    public Part centralPart;

    // The colony/craft we should search when looking for stuff like launchsites
    public Node3D activeThing;
    // Current active VAB.
    public VehicleAssembly activeVAB;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        SingletonRegistry.Register(this); // Register self
    }

    public override void _Process(double delta)
    {
        if (draggingPart != null)
        {
            Camera3D camera = ActiveSave.Instance.localCamera;

            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector3 projectedPosition = camera.ProjectPosition(mousePos, partHoldDistance);

            (AttachNode, AttachNode) attachNodeBuffer = (null, null);

            // Reset parent part in case we really don't want it
            draggingPart.parentPart = null;

            // Part Snappy
            foreach (Part part in partsList)
            {
                if (!draggingPart.descendantParts.Contains(part))
                {
                    foreach (AttachNode attachNode0 in part.attachNodes)
                    {
                        if (attachNode0.connectedNode == null)
                        {
                            // Iterate over the dragged part's nodes

                            foreach (AttachNode attachNode1 in draggingPart.attachNodes)
                            {
                                // We get screen positions to avoid depth problems
                                Camera3D localCam = ActiveSave.Instance.localCamera;

                                Vector3 node0Pos = attachNode0.GlobalPosition;
                                // Imagine this one (relative node position + supposed global position)
                                Vector3 node1Pos = attachNode1.GlobalPosition - draggingPart.GlobalPosition + projectedPosition;

                                Vector2 nodeScreenPos0 = localCam.UnprojectPosition(node0Pos);
                                Vector2 nodeScreenPos1 = localCam.UnprojectPosition(node1Pos);

                                float distance = (float)nodeScreenPos0.DistanceSquaredTo(nodeScreenPos1);

                                if (distance <= partSnapDistance*partSnapDistance)
                                {
                                    // Override the part's position with a new one !!
                                    projectedPosition = attachNode0.GlobalPosition - (attachNode1.GlobalPosition - draggingPart.GlobalPosition);
                                    // We remember those two nodes if we ever want to place the part again
                                    attachNodeBuffer = (attachNode0, attachNode1);
                                    // Track which part we attach to
                                    draggingPart.parentPart = attachNode0.part;
                                    break; // Exit the loop
                                }
                            }
                        }
                    }
                }
            }

            // We apply! !! !!! ! 1
            draggingPart.GlobalPosition = projectedPosition; 
            // Linear interp is broken at low FPS so bye bye !! draggingPart.GlobalPosition.Lerp(projectedPosition, partMoveSpeed);
            snappedNodes = attachNodeBuffer;
        }

        /*
        Godot.Collections.Array<Node> parts = editorPartContainer.GetChildren();

        // There's no concrete reason for a buffer but it makes some of this more manageable maybe?? uhhm uhh err :3
        List<Part> partListBuffer = [];
        foreach (Node node in parts)
        {
            if (node is Part part)
            {
                partListBuffer.Add(part);
            }
        }
        partsList = partListBuffer;
        
        */
        
        // Just pick one if it's null (the user will take control, otherwise)
        if (partsList.Count > 0 && centralPart == null)
        {
            centralPart = partsList[0];
            Logger.Print($"{classTag} Auto assigned central part to: {centralPart.Name}");
            centralPart.Position = new Vector3(0, 0, 0);
        }
    }

    public void InsertPart(CachedPart partRef)
    {
        Random RNG = new();

        Part part = partRef.Instantiate(floatingPartContainer, true, true);

        part.cachedPart = partRef;
        part.id = RNG.NextInt64();
        draggingPart = part;
    }

    public void ClearParts()
    {
        foreach (Part part in partsList)
        {
            part.QueueFree();
        }

        centralPart = null;
        partsList = [];
    }

    public void OpenBuildUI(bool open, bool selector = true)
    {
        buildUI.Visible = open;
        buildUI.partListContainer.Visible = open && selector;
        if (open && selector)
        {
            buildUI.partList.LoadPartList();
        }else{
            buildUI.partList.FreePartList();
        }
    }

    // Enter VAB build mode 
    public void EnterVAB(VehicleAssembly module, float maxZoom, float targetZoom)
    {
        // Assign VAB
        activeThing = module.part.parentThing;
        activeVAB = module;
        editorPartContainer.GlobalTransform = module.vab.GlobalTransform;
        floatingPartContainer.GlobalTransform = module.vab.GlobalTransform;

        // Reposition cam
        FlightCamera.Instance.TargetObject(module.camPivot, new Vector3(0.1f, maxZoom, targetZoom), false);
        editorMode = EditorMode.Static;

        // Disable map view while in flight
        FlightCamera.Instance.ToggleMapView(false);
        FlightCamera.Instance.canEnterMap = false;

        // Close Part Menus
        PartMenuHandler.Instance.contextMenus.OpenMenu("", [], true);

        // Initiate
        OpenBuildUI(true, true);
    }

    public void ExitBuildMode(bool returnFocus)
    {
        OpenBuildUI(false);

        // Close Part Menus
        PartMenuHandler.Instance.contextMenus.OpenMenu("", [], true);

        // Misc
        FlightCamera.Instance.canEnterMap = true;
        editorMode = EditorMode.None;

        // Return focus to the parent craft / colony IF WE WANT TO
        if (returnFocus)
        {
            if (activeThing is Colony colony)
            {
                FlightCamera.Instance.TargetObject(colony);
            }
        }
    }

    // Updates the hierarchy for each part
    public void UpdatePartHierarchy()
    {
        Logger.Print($"{classTag} Updated part hierarchies");
        foreach (Part part in partsList)
        {
            part.UpdateChildParts();
        }
    }

    // Function to delete parts in the editor context
    public void DeletePart(Part part)
    {
        Logger.Print($"{classTag} Deleting part {part.Name}");
        partsList.Remove(part);
        part.QueueFree();
        // Clear the current dragging part if we're throwing that out
        if (part == draggingPart) draggingPart = null;
        // Clear the central part if we're throwing that out too
        if (part == centralPart) centralPart = null;
    }

    // Inputs !!!
    public override void _UnhandledInput(InputEvent @event)
    {
        // Delete a part if dragging
        if (@event is InputEventKey key)
        {
            if (key.Pressed && key.Keycode == Key.Delete)
            {
                if (draggingPart != null)
                {
                    foreach (Part part in draggingPart.descendantParts)
                    {
                        DeletePart(part);
                    }

                    DeletePart(draggingPart);

                    if (verboseLogging)
                    {
                        Logger.Print($"{classTag} Part list:");

                        if (partsList.Count > 0)
                        {
                            foreach (Part part in partsList)
                            {
                                Logger.Print(part.Name);
                            }
                        }else{
                            Logger.Print("EMPTY");
                        }
                    }
                }
            }
        }

        if (@event is InputEventMouseButton mouseButt)
        {
            if (mouseButt.Pressed && mouseButt.ButtonIndex == MouseButton.Left)
            {
                if (draggingPart == null && PartManager.Instance.hoveredPart != null && PartManager.Instance.hoveredPart.inEditor) 
                {
                    draggingPart = PartManager.Instance.hoveredPart;
                    draggingPart.Reparent(floatingPartContainer);

                    // Fully detach, since we're taking the part off of others.
                    foreach (AttachNode attachNode in draggingPart.attachNodes)
                    {
                        attachNode.Detach();
                    }

                    Logger.Print($"{classTag} Selected part {draggingPart.Name}");
                    partsList.Remove(draggingPart);
                    UpdatePartHierarchy();
                }else if (draggingPart != null) {
                    if (draggingPart.parentPart != null)
                    {
                        // Parent to the parent part cuz we attach yay!!
                        draggingPart.Reparent(draggingPart.parentPart);
                        if (snappedNodes.Item1 != null && snappedNodes.Item2 != null)
                            snappedNodes.Item1.Attach(snappedNodes.Item2);

                        UpdatePartHierarchy();
                    }else{
                        // Parent to the part container cuz we didn't attach :(
                        draggingPart.Reparent(editorPartContainer);
                    }

                    Logger.Print($"{classTag} Unselected part {draggingPart.Name}");
                    partsList.Add(draggingPart);
                    draggingPart = null;
                }
            }
        }
    }
}
