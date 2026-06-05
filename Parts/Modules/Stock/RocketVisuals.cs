using Godot;

public partial class RocketVisuals : PartModule
{
    public Node3D ParticlesNode { get; private set; }

    public override void PartInit() 
    {
        string nodeName = (string)configData["particlesNodeName"];

        ParticlesNode = (Node3D)part.FindChild(nodeName);
    }

    public override void PartProcess()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (part.parentThing is Craft craft)
        {
            float val = (float)(1 - craft.Throttle);
            foreach (Node node in ParticlesNode.GetChildren())
            {
                if (node is GpuParticles3D particles)
                {
                    particles.Transparency = val;
                }
            }
        }
    }
}
