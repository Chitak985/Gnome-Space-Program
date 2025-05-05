using Godot;
using System;
using System.Collections.Generic;

public partial class Craft : Node
{
    public SavedPart rootPart;
    public List<SavedPart> parts;
    public List<PartDefinition> instancedParts;

    public void Instantiate(Node parent)
    {
        instancedParts = [];
        foreach (SavedPart part in parts)
        {
            PartDefinition newPart = (PartDefinition)part.reference.Instantiate();
            parent.AddChild(newPart);
            newPart.GlobalPosition = part.position;
            newPart.GlobalRotationDegrees = part.rotation;
            newPart.inEditor = false;
            newPart.Freeze = true;
            instancedParts.Add(newPart);
        }
        PartDefinition lowestPart = GetLowestPart();
        Vector3 lowestPartPos = lowestPart.Position;
        foreach(PartDefinition part in instancedParts)
        {
            part.Position -= lowestPartPos;
        }
    }

    public void CreateJoints(Node parent)
    {
        if (instancedParts != null)
        {
            foreach (PartDefinition part0 in instancedParts)
            {
                foreach (AttachNode node0 in part0.attachNodes)
                {
                    (AttachNode node1, PartDefinition part1) = GetClosestAttach(node0);
                    GD.Print(node1);
                    if (node1 != null)
                    {
                        ConeTwistJoint3D joint = new(){
                            SwingSpan = 0f,
                            TwistSpan = 0f,
                            NodeA = part0.GetPath(),
                            NodeB = part1.GetPath()
                        };
                        part0.AddChild(joint);
                        joint.Position = new Vector3(0,0,0);
                    }
                }
            }
        }else{
            GD.PrintErr("Parts have not been instanced yet!");
        }
    }

    public void FreezeCraft(bool status)
    {
        if (instancedParts != null)
        {
            foreach (PartDefinition part in instancedParts)
            {
                part.Freeze = status;
            }
        }else{
            GD.PrintErr("Parts have not been instanced yet!");
        }
    }

    public void TogglePartModules(bool status)
    {
        if (instancedParts != null)
        {
            foreach (PartDefinition part in instancedParts)
            {
                foreach (PartModule module in part.partModules)
                {
                    module.enabled = status;
                }
            }
        }else{
            GD.PrintErr("Parts have not been instanced yet!");
        }
    }

    private (AttachNode, PartDefinition) GetClosestAttach(AttachNode node0)
    {
        AttachNode closest = null;
        PartDefinition closestPart = null;
        float closestDist = float.PositiveInfinity;
        foreach (PartDefinition part in instancedParts)
        {
            foreach (AttachNode node1 in part.attachNodes)
            {
                float nodeDist = node0.GlobalPosition.DistanceTo(node1.GlobalPosition);
                if (nodeDist < closestDist && node0 != node1)
                {
                    closest = node1;
                    closestPart = part;
                    closestDist = nodeDist;
                }
            }
        }
        GD.Print(closestDist);
        if (closestDist < 0.5f)
        {
            return (closest, closestPart);
        }else{
            return (null, null);
        }
    }

    private PartDefinition GetLowestPart()
    {
        Vector3 lowest = new Vector3(0,float.PositiveInfinity,0);
        PartDefinition lowestPart = null;
        foreach (PartDefinition part in instancedParts)
        {
            if (part.Position.Y < lowest.Y)
            {
                lowest = part.Position;
                lowestPart = part;
            }
        }
        return lowestPart;
    }
}