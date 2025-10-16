using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class MapRoom : Resource
{
    [Export] public string RoomTitle { get; set; } = "";
    [Export] public string RoomId { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Godot.Collections.Array<string> Items { get; set; } = new();
    [Export] public Godot.Collections.Array<string> Encounters { get; set; } = new();
    [Export] public Godot.Collections.Dictionary<string, string> Exits { get; set; } = new();
    // e.g., { "north": "room_2" }
}
