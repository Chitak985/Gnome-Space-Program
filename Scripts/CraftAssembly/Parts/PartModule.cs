using Godot;
using Godot.Collections;
using System;

// Every part module should inherit from this
public partial class PartModule
{
    public Part part;
    public Dictionary configData;

    // Called when a part is "started" i guess
    public virtual void PartInit() {}

    public virtual Dictionary FetchData() { return null; }
}
