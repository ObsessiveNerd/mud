using Godot;
using System;

public partial class LineEditBehavior : LineEdit
{
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                // Consume the event so LineEdit doesn't process it
                AcceptEvent();
                Text = "";
                return;
            }
        }

        base._GuiInput(@event);
    }
}
