using Godot;
using System;
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

		NorthButton.Pressed += () => TryCreateOrRemoveRoomInDirection("north");
		SouthButton.Pressed += () => TryCreateOrRemoveRoomInDirection("south");
		EastButton.Pressed += () => TryCreateOrRemoveRoomInDirection("east");
		WestButton.Pressed += () => TryCreateOrRemoveRoomInDirection("west");

		// Create a starting room
		var start = AddNewRoom("Room_0", new Vector2(0, 0));
		Graph.SetSelected(start);
	}

	private GraphNode AddNewRoom(string roomId, Vector2 offset)
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

		_nodeToRoom[node] = newRoom;
		Graph.AddChild(node);

		return node;
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

		if (_selectedRoom.Exits.ContainsKey("north"))
		{
			UpdateButton(NorthButton, "Clear North");
		}
		else
		{
			UpdateButton(NorthButton, "Add North");	
		}

		if (_selectedRoom.Exits.ContainsKey("south"))
		{
			UpdateButton(SouthButton, "Clear South");
		}
		else
		{
			UpdateButton(SouthButton, "Add South");
		}

		if (_selectedRoom.Exits.ContainsKey("east"))
		{
			UpdateButton(EastButton, "Clear East");
		}
		else
		{
			UpdateButton(EastButton, "Add East");
		}

		if (_selectedRoom.Exits.ContainsKey("west"))
		{
			UpdateButton(WestButton, "Clear West");
		}
		else
		{
			UpdateButton(WestButton, "Add West");
		}
	}
	
	void UpdateButton(Button button, string newText)
    {
		button.Text = newText;
    }

	private void TryCreateOrRemoveRoomInDirection(string direction)
	{
		if (_selectedRoom == null)
			return;

		// Check if that exit already exists, if so delete the connection
		if (_selectedRoom.Exits.ContainsKey(direction))
		{
			DisconnectRooms(_selectedRoom, direction);
			SelectRoom(_selectedRoom);
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

		var checkForExistingNode = GetNodeByOffset(newOffset);
		if(checkForExistingNode != null)
        {
			MapRoom connectToRoom = _nodeToRoom[checkForExistingNode];
			ConnectRooms(_selectedRoom, connectToRoom, direction);
			Graph.SetSelected(GetNodeForRoom(connectToRoom));
			return;
        }

		string newRoomId = $"{_selectedRoom.RoomId}_{direction}";
		var newRoom = AddNewRoom(newRoomId, newOffset);

		ConnectRooms(_selectedRoom, _nodeToRoom[newRoom], direction);
		Graph.SetSelected(newRoom);
	}

	private void DisconnectRooms(MapRoom baseRoom, string direction)
    {
		MapRoom connectingRoom = _currentMap.Rooms[baseRoom.Exits[direction]];
		baseRoom.Exits.Remove(direction);
		string otherDirection = GetOppositeDirection(direction);
		connectingRoom.Exits.Remove(otherDirection);
    }

	private void ConnectRooms(MapRoom from, MapRoom to, string direction)
	{
		from.Exits[direction] = to.RoomId;

		string opposite = GetOppositeDirection(direction);
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

	private GraphNode GetNodeByOffset(Vector2 offset)
    {
        foreach (var graphNode in _nodeToRoom.Keys)
		{
			if (graphNode.PositionOffset == offset)
				return graphNode;
		}
		return null;
    }

	string GetOppositeDirection(string direction)
    {
		string opposite = direction switch
		{
			"north" => "south",
			"south" => "north",
			"east" => "west",
			"west" => "east",
			_ => ""
		};
		return opposite;
    }
}
