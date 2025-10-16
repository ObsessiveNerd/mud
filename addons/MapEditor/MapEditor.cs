using Godot;
using System.Collections.Generic;

[Tool]
public partial class MapEditor : Control
{
    [Export] private GraphEdit Graph;
    [Export] private Button NorthButton;
    [Export] private Button SouthButton;
    [Export] private Button EastButton;
    [Export] private Button WestButton;

    private MapData _currentMap = new();
    private Dictionary<GraphNode, MapRoom> _nodeToRoom = new();
    private MapRoom _selectedRoom;
    private const float ROOM_SPACING = 100f; // Distance between rooms

    public override void _Ready()
    {
        Graph.NodeSelected += OnNodeSelected;

        // Disable GraphEdit drag and zoom
        Graph.ScrollOffset = Vector2.Zero;
        Graph.Zoom = 1f;
        Graph.ShowGrid = true;
        Graph.MinimapEnabled = false;

        NorthButton.Pressed += () => TryCreateRoomInDirection("north");
        SouthButton.Pressed += () => TryCreateRoomInDirection("south");
        EastButton.Pressed += () => TryCreateRoomInDirection("east");
        WestButton.Pressed += () => TryCreateRoomInDirection("west");

        // Create a starting room
        var start = AddNewRoom("Room_0", new Vector2(0, 0));
        SelectRoom(start);
    }

    private MapRoom AddNewRoom(string roomId, Vector2 offset)
    {
        var newRoom = new MapRoom { RoomId = roomId };
        _currentMap.Rooms.Add(roomId, newRoom);

        var node = new GraphNode
        {
            Name = roomId,
            Title = roomId,
            PositionOffset = offset,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            Draggable = false // 🚫 prevent node dragging
        };
        
        node.AddThemeConstantOverride("minimum_width", 100);
        node.AddThemeConstantOverride("maximum_width", 100);

        Graph.AddChild(node);
        _nodeToRoom[node] = newRoom;

        return newRoom;
    }

    private void OnNodeSelected(Node node)
    {
        if (node is GraphNode graphNode && _nodeToRoom.TryGetValue(graphNode, out var room))
        {
            SelectRoom(room);
        }
    }

    private void SelectRoom(MapRoom room)
    {
        _selectedRoom = room;
        GD.Print($"Selected room: {room.RoomId}");
    }

    private void TryCreateRoomInDirection(string direction)
    {
        if (_selectedRoom == null)
            return;

        // Check if that exit already exists
        if (_selectedRoom.Exits.ContainsKey(direction))
        {
            GD.Print($"{_selectedRoom.RoomId} already has a {direction} exit!");
            return;
        }

        var fromNode = GetNodeForRoom(_selectedRoom);
        if (fromNode == null)
            return;

        var baseOffset = fromNode.PositionOffset;
        Vector2 newOffset = direction switch
        {
            "north" => baseOffset + new Vector2(0, -ROOM_SPACING),
            "south" => baseOffset + new Vector2(0, ROOM_SPACING),
            "east" => baseOffset + new Vector2(ROOM_SPACING * 2, 0),
            "west" => baseOffset + new Vector2(-ROOM_SPACING * 2, 0),
            _ => baseOffset
        };

        string newRoomId = $"{_selectedRoom.RoomId}_{direction}";
        var newRoom = AddNewRoom(newRoomId, newOffset);

        ConnectRooms(_selectedRoom, newRoom, direction);
        SelectRoom(newRoom);
    }

    private void ConnectRooms(MapRoom from, MapRoom to, string direction)
    {
        from.Exits[direction] = to.RoomId;

        string opposite = direction switch
        {
            "north" => "south",
            "south" => "north",
            "east" => "west",
            "west" => "east",
            _ => ""
        };
        to.Exits[opposite] = from.RoomId;

        var fromNode = GetNodeForRoom(from);
        var toNode = GetNodeForRoom(to);
        if (fromNode != null && toNode != null)
        {
            //Graph.ConnectNode(fromNode.Name, 1, toNode.Name, 1);
        }
    }

    private GraphNode GetNodeForRoom(MapRoom room)
    {
        foreach (var kvp in _nodeToRoom)
        {
            if (kvp.Value == room)
                return kvp.Key;
        }
        return null;
    }
}