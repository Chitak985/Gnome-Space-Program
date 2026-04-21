using Godot;

public partial class CartesianState : Node
{
    public CartesianElements elements;

    public struct CartesianElements
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;
    }
}
