#if TOOLS
using Godot;

[Tool]
public partial class TextMapEditorPlugin : EditorPlugin
{
    private Control _mapEditor;

    public override void _EnterTree()
    {
        _mapEditor = GD.Load<PackedScene>("res://addons/MapEditor/MapEditor.tscn").Instantiate<Control>();
        AddControlToDock(DockSlot.RightUl, _mapEditor);
        GD.Print("TextMap Editor loaded!");
    }

    public override void _ExitTree()
    {
        RemoveControlFromDocks(_mapEditor);
        _mapEditor.Free();
    }
}
#endif
