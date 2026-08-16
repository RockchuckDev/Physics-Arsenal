using Godot;
using System;

public partial class DisplayGrabMode : Label
{
    /// <summary> Updates the Grab Mode UI element to show the grab mode the player has active</summary>
    public void OnGrabModeChanged(bool isToggleGrabMode)
    {
        Text = isToggleGrabMode ? "Grab Mode: Toggle" : "Grab Mode: Hold";
    }
}
