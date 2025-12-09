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
    // Parts that are attached to this part
    public List<Part> childParts = [];
    // ALL parts that descend from this part
    public List<Part> descendantParts = [];

    public bool overrideHover = false;

    public override void _Process(double delta)
    {
        if (PartMenuHandler.Instance != null && !overrideHover)
        {
            Highlight(PartManager.Instance.hoveredPart == this, true); 
        }
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

    public void ReadData(Dictionary data)
    {
        Position = (Vector3)data["position"];
        RotationDegrees = (Vector3)data["rotation"]; // relative to parent attachment
        id = (int)data["partID"];

        Dictionary attachmentData = (Dictionary)data["attachments"];
        foreach (KeyValuePair<Variant, Variant> node in attachmentData)
        {
            // IMplement lateor
        }
    }

    public Dictionary GetData()
    {
        Dictionary data = [];

        // Throw basic info into here
        data.Add("position", Position);
        data.Add("rotation", RotationDegrees);
        data.Add("partID", id);

        // Index - Index of attachment node HERE
        // Key - ID of the other part
        // Value - Index of the OTHER attachment node
        Dictionary attachmentData = [];
        foreach (AttachNode node in attachNodes)
        {
            if (node.connectedNode != null)
            {
                attachmentData.Add(node.connectedNode.part.id, node.connectedNode.part.attachNodes.IndexOf(node.connectedNode));
            }
        }

        data.Add("attachments", attachmentData);

        // Fetch data from every part module
        // IMPLEMENT C# TOO SOMEDAY pppwwweeaaaaaseeeeeee 
        Dictionary moduleDataContainer = [];
        //foreach (Node module in partModules)
        //{
        //    Dictionary moduleData = (Dictionary)module.Call("getData");
        //    moduleDataContainer.Add(module.GetScript(), moduleData);
        //}

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
