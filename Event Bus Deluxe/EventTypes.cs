using Godot;

/// <summary> Fires when the player presses the interact button for a given hand. Only the objects that are actively being held and have a child node that defines interaction behavior are subscribed to this event </summary>
public struct PlayerRequestedToInteractWithGrabbedObjectEvent
{
    /// <summary> The GrabData of the hand that is requesting to interact with the grabbed object </summary>
    public GrabData requestedHandDataToInteractWithGrabbedObject;
}

/// <summary> Fires when the player successfully interacts with a grabbed object.</summary>
public struct PlayerSuccessfullyInteractedWithGrabbedObjectEvent
{
    /// <summary> The GrabData for the hand that was holding the object that was interacted with. </summary>
    public GrabData handDataWhichWasHoldingObjectThatWasInteractedWith;
}

public struct SpawnBulletImpactParticlesEvent
{
    /// <summary> Position in world space to spawn the particles at</summary>
    public Vector3 impactPoint;
    /// <summary> Essentially the "ricochet" direction of the impact point. </summary>
    public Vector3 impactNormal;
    /// <summary> The particles to spawn in </summary>
    public PackedScene bulletImpactParticlesScene;
}

/// <summary> Fires when the player changes their grab mode. </summary>
public struct PlayerChangedGrabModeEvent
{
    /// <summary> Boolean to represent if the active grab mode is "toggle" (true) or is "hold" (false) </summary>
    public bool isToggleGrabModeOn;
}
