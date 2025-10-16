using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class MapData : Resource
{
    [Export] public Godot.Collections.Dictionary<string, MapRoom> Rooms = new Godot.Collections.Dictionary<string, MapRoom>();
}
