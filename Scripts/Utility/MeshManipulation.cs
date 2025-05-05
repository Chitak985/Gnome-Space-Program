using Godot;
using System;

public partial class MeshManipulation : Node
{
    public static Godot.Collections.Array getMeshData(Mesh mesh)
    {
        return mesh.SurfaceGetArrays(0);
    }

    public static Vector3[] calculateSmoothNormals(Vector3[] vertices, int[] indices)
    {
        Vector3[] newNormals = new Vector3[vertices.Length];
        // Normals
        //for (int i = 0; i < newNormals.Length; i++)
        //{
        //    newNormals[i] = Vector3.Zero;
        //}
        for (int i = 0; i < indices.Length; i++)
        {
            if (i <= indices.Length - 3)
            {
                int index0 = (int)indices[i];
                int index1 = (int)indices[i + 1];
                int index2 = (int)indices[i + 2];

                Vector3 v0 = vertices[index0];
                Vector3 v1 = vertices[index1];
                Vector3 v2 = vertices[index2];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                Vector3 normal = edge1.Cross(edge2).Normalized();

                newNormals[index0] += -normal;
                newNormals[index1] += -normal;
                newNormals[index2] += -normal;
            }
        }
        return newNormals;
    }
}