using Godot;
using System;

/// <summary> Moves a control node to the point on the screen that the raycast collision or end point is projected onto. </summary>
public partial class MoveToRaycastPointOnScreen : ColorRect
{
    /// <summary> The raycast this color rect tracks </summary>
    [Export]
    public RayCast3D raycast;
    private Camera3D activeCamera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        activeCamera = GetViewport().GetCamera3D();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Position = activeCamera.UnprojectPosition(raycast.GetCollisionPointOrEndPoint());
    }
}
