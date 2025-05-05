using Godot;
using System;
using System.Collections.Generic;
using System.IO.Pipes;

public partial class ListManipulators
{
    public static List<List<Vector3>> ChunkList(List<Vector3> list, int rowLength)
    {
        List<List<Vector3>> newList = [];
        for (int y = 0; y < rowLength; y++)
        {
            List<Vector3> row = [];
            for (int x = 0; x < rowLength; x++)
            {
                row.Add(list[x*y]);
            }
            newList.Add(row);
            //GD.Print(row.Count);
        }
        //GD.Print(newList.Count);
        return newList;
    }
}
