using Godot;
using System;

public partial class ShootOnInteract : RayCast3D
{
    /// <summary> The root object node of this grabbable object </summary>
    [Export]
    public Node rootObjectNode;

    /// <summary> How fast the bullet travels. Only used in calculating how much force is applied to objects that are shot </summary>
    [Export]
    public float bulletSpeed;

    /// <summary> The mass of the bullet. Only used in calculating how much force is applied to objects that are shot </summary>
    [Export]
    public float bulletMass;

    /// <summary> The range of the gun</summary>
    [Export]
    public float range = 100;

    /// <summary> The GpuParticles3D node that is spawned when an object (static or dynamic) is shot </summary>
    [Export]
    public PackedScene bulletImpactParticles;

    /// <summary> Whether or not to show a crosshair on the screen for this gun </summary>
    [Export]
    public bool showCrosshair;

    /// <summary> The scene of the crosshair that is shown on the screen for this gun (provided showCrosshair is true) </summary>
    [Export]
    public PackedScene crosshairScene;


    private Control activeCrosshair;

    private Camera3D playerCamera;

    public override void _Ready()
    {
        playerCamera = GetViewport().GetCamera3D();
        if (rootObjectNode.FindChildOfType<IsGrabbedTracker>() != null)
        {
            rootObjectNode.FindChildOfType<IsGrabbedTracker>().IsGrabbedTrackerUpdated += SetScreenSightVisibility;
            rootObjectNode.FindChildOfType<IsGrabbedTracker>().IsGrabbedTrackerUpdated += SubscribeOrUnsubscribeToPlayerRequestInteractOnIsGrabbedTrackerUpdate;
        }

    }

    public override void _Process(double delta)
    {
        if (rootObjectNode.FindChildOfType<IsGrabbedTracker>()?.isGrabbed == true)
        {
            activeCrosshair.Position = playerCamera.UnprojectPosition(this.GetCollisionPointOrEndPoint());

        }

    }

    /// <summary> Subscribed to the IsGrabbedTracker "IsGrabbedTrackerUpdated" signal. If the provided bool is true (and showCrosshair is true), the crosshair for this gun is shown on screen, if false, the crosshair is hidden  </summary>
    public void SetScreenSightVisibility(bool isVisible)
    {
        if(showCrosshair){
            if(isVisible){
                activeCrosshair = crosshairScene.Instantiate<Control>();
                GetNode("/root/CrosshairManager").AddChild(activeCrosshair);
            }else{
                activeCrosshair.QueueFree();
            }
        }



    }

    /// <summary> When the IsGrabbedTrackerUpdated signal fires, if the provided bool is true, subscribe OnPlayerRequestInteract to the custom <see cref="PlayerRequestedToInteractWithGrabbedObjectEvent"/> event bus, and unsubscribe if it is false. </summary>
    public void SubscribeOrUnsubscribeToPlayerRequestInteractOnIsGrabbedTrackerUpdate(bool grabState)
    {

        if (grabState == true)
        {
            EventBusDeluxe.Subscribe<PlayerRequestedToInteractWithGrabbedObjectEvent>(OnPlayerRequestInteract);

        }
        else
        {
            EventBusDeluxe.Unsubscribe<PlayerRequestedToInteractWithGrabbedObjectEvent>(OnPlayerRequestInteract);
        }

    }

    /// <summary> Apply force to the object that gets shot, and tell <see cref="ParticleManager"/> to spawn the provided impact particles </summary>
    public void OnPlayerRequestInteract(PlayerRequestedToInteractWithGrabbedObjectEvent eventData)
    {
        GrabData handData = eventData.requestedHandDataToInteractWithGrabbedObject;
        if (handData.grabbedObject == rootObjectNode && IsColliding())
        {
            if (GetCollider() is RigidBody3D rigidbody)
            {
                rigidbody.ApplyImpulse(-GlobalBasis.Z * bulletSpeed * bulletMass, GetCollisionPoint());
            }

            EventBusDeluxe.Fire<SpawnBulletImpactParticlesEvent>(new SpawnBulletImpactParticlesEvent{
                impactPoint = GetCollisionPoint(),
                impactNormal = GetCollisionNormal(),
                bulletImpactParticlesScene = bulletImpactParticles
            });

        }


    }
}
