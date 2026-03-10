using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/* 
Technically this class encompasses both colony AND ship parts, as I intend for them to be used interchangeably.
Why? Because I want players to have the freedom to get up to any sort of shenanigans with these systems.
*/
public partial class Part : RigidBody3D
{
    [Export] public bool enabled = false;
    [Export] public Material glowMat;
    [Export] public MeshInstance3D glowMesh;
    [Export] public bool selectable = true;

    // Whether or not to treat the part as "not real" (in the editor)
    [Export] public bool inEditor;

    [Export] public Array<AttachNode> attachNodes;

    // What to copy over to the craft upon intantiation
    [Export] public Array<CollisionShape3D> colliders;

    [Signal] public delegate void SendButtonEventHandler(string name);

    public CachedPart cachedPart;
    // Craft or Colony
    public Node3D parentThing;

    public List<PartModule> partModules = [];

    // Assigned by stufffy like the part picker in the editor
    public long id;

    public PartMenu contextMenu;

    // What this part is attached to
    public Part parentPart;
    // Attached node (parent)
    public AttachNode parentNode;
    // Attached node (current)
    public AttachNode usedNode;
    // Parts that are attached to this part
    public List<Part> childParts = [];
    // ALL parts that descend from this part
    public List<Part> descendantParts = [];
    // Container for the 3D joints that attach the part (null if not connected)
    public Node3D attachmentJointContainer;

    public bool overrideHover = false;

    public override void _Ready()
    {
        InputEvent += OnInputEvent;
        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;

        CustomIntegrator = true;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (parentThing is Craft craft)
        {
            CelestialBody orbitingBody = craft.OrbitDriver.parent;
            double bodyMass = orbitingBody.mass;

            Vector3 center = orbitingBody.GlobalPosition;
            Vector3 direction = GlobalPosition.DirectionTo(center);
            double distance = (center - GlobalPosition).Length();
            double force = Conics.GravConstant * (bodyMass * Mass / Mathf.Pow(distance, 2));

            state.LinearVelocity += direction * force * GetProcessDeltaTime();
        }
    }

    private void OnInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
            {
                PartMenuHandler.Instance.ToggleMenu(this);
            }
        }
    }

    private void OnMouseEnter()
    {
        Highlight(true, true); 
    }

    private void OnMouseExit()
    {
        Highlight(false, true); 
    }

    public void UpdateChildParts()
    {
        childParts = GetChildParts(false);
        descendantParts = GetChildParts(true);
    }

    // Get every part that descends from this one
    public List<Part> GetChildParts(bool recursive)
    {
        List<Part> result = [];

        foreach (Node node in GetChildren())
        {
            if (node is Part part)
            {
                result.Add(part); // Add the immediate part

                // We go all the way down the hierarchy if we want I guess (and pray that we don't end up in a loop)
                if (recursive)
                {
                    List<Part> childResults = part.GetChildParts(recursive);
                    result.AddRange(childResults); // Add all of its children too
                }
            }
        }

        return result;
    }

    public void InitPart()
    {
        contextMenu = PartMenuHandler.Instance.CreateMenu(this);
        Godot.Collections.Array moduleData = (Godot.Collections.Array)cachedPart.config["modules"];
        Logger.Print($"(Instance {Name}) Creating part modules...");
        partModules = CreateModules(moduleData);
        Logger.Print($"(Instance {Name}) Got all part modules! Count: {partModules.Count}");
        //InitModules();
    }
    
    public List<PartModule> CreateModules(Godot.Collections.Array data)
    {
        List<PartModule> modules = [];
        
        foreach (Variant mod in data)
        {
            if (mod.VariantType == Variant.Type.Dictionary)
            {
                Dictionary modData = (Dictionary)mod;

                string moduleName = (string)modData["type"];
                Type moduleType = PartManager.Instance.partModules[moduleName];

                // create an object of the type
                PartModule module = (PartModule)Activator.CreateInstance(moduleType);
                module.part = this;
                module.configData = modData;

                modules.Add(module);

                module.PartInit();
            }
        }

        return modules;
    }

    // recursive function that returns a tree descending from this part
    // reconstruction isn't handled by individual parts so look into Craft.cs or Colony.cs for methods that do that
    public Dictionary GetData()
    {
        Dictionary data = [];

        // Throw basic info into here
        data.Add("name", cachedPart.name);
        data.Add("position", Position); // should be relative to attach node
        data.Add("rotation", RotationDegrees);
        data.Add("partID", id);
    
        // This will be -1 for the topmost part so be careful
        // Index of parent's attach node
        if (parentPart != null)
        {
            data.Add("parentNode", parentPart.attachNodes.IndexOf(parentNode));
            data.Add("usedNode", attachNodes.IndexOf(usedNode));
        } else {
            data.Add("parentNode", -1);
            data.Add("usedNode", -1);
        }

        Godot.Collections.Array childPartData = [];
        foreach (Part part in childParts)
        {
            // Index of the attachment node, part data
            childPartData.Add(part.GetData());
        }
        data.Add("attachedParts", childPartData);

        // Fetch data from every part module
        Godot.Collections.Array moduleDataContainer = [];
        foreach (PartModule module in partModules)
        {
            Dictionary moduleData = module.FetchData();
            if (moduleData != null)
            {
                moduleDataContainer.Add(moduleData);
            }
        }

        data.Add("modules", moduleDataContainer);

        return data;
    }

    public void Highlight(bool toggle, bool includeChildren = false)
    {
        if (glowMesh != null)
        {
            if (toggle)
            {
                glowMesh.MaterialOverlay = glowMat;
            }else{
                glowMesh.MaterialOverlay = null;
            }
        }

        // Make child parts glow too
        if (includeChildren || !toggle)
        {
            foreach (Part part in descendantParts)
            {
                part.overrideHover = toggle;
                part.Highlight(toggle);
            }
        }
    }

    // Toggle to make this part have physics or not i guess
    public void Anchor(bool toggle)
    {
        Freeze = toggle;
        LockRotation = toggle;
        TopLevel = !toggle;
    }

    // Recursive function to get all meshes
    public List<MeshInstance3D> GetMeshes(Node node = null)
    {
        node ??= this;

        List<MeshInstance3D> meshList = [];

        foreach(Node child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                meshList.Add(mesh);
            }

            if (child.GetChildCount() > 0)
            {
                List<MeshInstance3D> meshBuffer = GetMeshes(child);
                meshList.AddRange(meshBuffer);
            }
        }

        return meshList;
    }

    // Node0 = this node Node1 = other node
    public void CreateAttachJoints(AttachNode node0, AttachNode node1)
    {
        if (attachmentJointContainer == null)
        {
            Node3D container = new();
            AddChild(container);
            container.Name = "JointContainer";
            container.Position = node0.Position;

            // Piece of shit

            Generic6DofJoint3D joint0 = new()
            {
                NodeA = GetPath(),
                NodeB = node1.part.GetPath(),
            };
            container.AddChild(joint0);

        }else{
            Logger.Print($"({cachedPart.name}) Joints already present! Cannot create!");
        }
    }

    public void DestroyAttachJoints()
    {
        if (attachmentJointContainer != null)
        {
            attachmentJointContainer.QueueFree();
        }else{
            Logger.Print($"({cachedPart.name}) Joints already present! Cannot destroy!");
        }
    }

    public Aabb GetAABB()
    {
        List<MeshInstance3D> meshList = GetMeshes(this);

        Aabb aabb = meshList[0].GetAabb();

        foreach (MeshInstance3D mesh in meshList)
        {
            //Logger.Print($"{Name} {mesh.GetAabb()}");
            aabb.Merge(mesh.GetAabb());
        }

        return aabb;
    }

    public List<PartModule> GetPartModules(Type filter = null)
    {
        if (filter == null)
        {
            // We don't need to filter anything in this case
            return partModules;
        }else{
            List<PartModule> modules = [];

            foreach (PartModule module in partModules)
            {
                if (module.GetType() == filter) modules.Add(module);
            }

            return modules;
        }
    }
}
