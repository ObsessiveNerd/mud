using Godot;
using System;
using System.Collections.Generic;
using static Godot.HttpClient;

public partial class NetworkManager : Node
{
    private const int Port = 7777;
    private const int MaxClients = 8;
    private ENetMultiplayerPeer _peer;

    [Export] Godot.Collections.Dictionary<int, Character> players = new Godot.Collections.Dictionary<int, Character>();

    public override void _Ready()
    {
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
    }

    // Start a server
    public void StartServer()
    {
        if (Globals.CurrentCharacter == null)
        {
            GetParent<Terminal>().Output("You must create or load a character before connecting to a server.");
            return;
        }

        GD.Print("Starting server...");

        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateServer(Port, MaxClients);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to start server: {error}");
            return;
        }

        Multiplayer.MultiplayerPeer = _peer;
        players.Add(Multiplayer.GetUniqueId(), Globals.CurrentCharacter);
        GD.Print($"Server started on port {Port}");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    void RequestCharacterList()
    {
        Rpc(MethodName.SyncCharacterList);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    void AddCharacter(int id, byte[] c)
    {
        UpdateCharacterList(id, c);
        SyncCharacterList(); // Notify all clients to sync character list
    }

    void SyncCharacterList()
    {
        foreach(var id in players.Keys)
        {
            GetParent<Terminal>().Output($"Syncing character list with {id}...");

            var character = players[id];
            byte[] playersData = FileAccess.GetFileAsBytes(character.ResourcePath);
            Rpc(MethodName.UpdateCharacterList, id, playersData);
        }
    }

    [Rpc]
    void UpdateCharacterList(int id, byte[] c)
    {
        string path = $"res://Characters/Character_{id}.tres";

        var f = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        f.StoreBuffer(c);
        f.Close();
        
        Character character = GD.Load<Character>(path);
        players[id] = character;
    }

    // Connect as client
    public void ConnectToServer(string address = "127.0.0.1")
    {
        if(Globals.CurrentCharacter == null)
        {
            GetParent<Terminal>().Output("You must create or load a character before connecting to a server.");
            return;
        }

        GD.Print($"Connecting to {address}:{Port}...");

        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateClient(address, Port);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to connect: {error}");
            return;
        }

        Multiplayer.MultiplayerPeer = _peer;
        Multiplayer.ConnectedToServer += () => RpcId(1, MethodName.AddCharacter, Multiplayer.GetUniqueId(), 
            FileAccess.GetFileAsBytes(Globals.CurrentCharacter.ResourcePath));

    }

    // Disconnect
    public void Disconnect()
    {
        _peer?.Close();
        Multiplayer.MultiplayerPeer = null;
        GD.Print("Disconnected.");
    }

    public Character GetMyCharacter()
    {
        if (players.ContainsKey(Multiplayer.GetUniqueId()))
            return players[Multiplayer.GetUniqueId()];
        return null;
    }

    public Godot.Collections.Dictionary<int, Character>  GetAllCharacters()
    {
        return players;
    }

    // Callbacks
    private void OnPeerConnected(long id)
    {
        GD.Print($"Peer connected: {id}");
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Peer disconnected: {id}");
    }

    private void OnConnectedToServer()
    {
        GD.Print("Successfully connected to server!");
    }

    private void OnConnectionFailed()
    {
        GD.PrintErr("Failed to connect to server.");
    }

    // Example RPC call
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void _SayHello(string message)
    {
        GD.Print($"[RPC] {Multiplayer.GetUniqueId()}: {message}");
    }

    // Example: send message to everyone
    public void SendHello()
    {
        Rpc("_SayHello", "Hello from " + Multiplayer.GetUniqueId());
    }
}
