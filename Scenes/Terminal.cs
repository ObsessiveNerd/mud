using Godot;
using System;

public enum TerminalContext
{
    Menu,
    Game,
}

public partial class Terminal : Node2D
{
    [Export]
    LineEdit terminalInput;

    [Export]
    RichTextLabel terminalOutput;

    [Export]
    InputParser parser;

    [Export]
    TerminalContext context = TerminalContext.Menu;

    [Export]
    NetworkManager networkManager;

    override public void _Ready()
    {
        terminalInput.GrabFocus();
        parser.networkManager = networkManager;
        networkManager.Multiplayer.PeerConnected += (id) => 
        {
            GD.Print($"Peer connected: {id}");
            terminalOutput.Text += $"\n> [color=blue]Peer connected: {id}[/color]";
        };
    }

    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("ui_accept"))
        {
            string inputText = terminalInput.Text.Trim();
            if (!string.IsNullOrEmpty(inputText))
            {
                terminalOutput.Text += $"\n> {inputText}";
                terminalInput.Text = string.Empty;
            }

            string response = parser.ParseInput(context, inputText);

            if (!string.IsNullOrEmpty(response))
            {
                terminalOutput.Text += $"\n> [color=green]{response}[/color]";
            }
            else
            {
                string name = networkManager.GetMyCharacter()?.Name;
                Rpc(MethodName.Speak, $"\n> {name}: {inputText}");
            }
        }
    }

    public void Output(string message)
    {
        terminalOutput.Text += $"\n> {message}";
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    void Speak(string message)
    {
        terminalOutput.Text += message;
    }
}
