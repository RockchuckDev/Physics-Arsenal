using Godot;
using System;

public partial class IsGrabbedTracker : Node
{
    [Export]
    public bool isGrabbed;

    [Signal]
    public delegate void IsGrabbedTrackerUpdatedEventHandler(bool currentGrabState);

    /// <summary> Updates the isGrabbed bool, emits the IsGrabbedTrackerUpdated(bool newGrabState) signal. </summary>
    public void UpdateGrabTracker(bool newGrabState)
    {
        GD.Print("update grab tracker: " + newGrabState);
        isGrabbed = newGrabState;
        EmitSignal(SignalName.IsGrabbedTrackerUpdated, isGrabbed);
    }

}
