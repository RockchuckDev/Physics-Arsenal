using Godot;

public partial class ParticleManager : Node
{
    public override void _Ready()
    {
        EventBusDeluxe.Subscribe<SpawnBulletImpactParticlesEvent>(SpawnBulletParticles);
    }

    /// <summary> A public method that is subscribed to the <see cref="SpawnBulletImpactParticlesEvent"/> and spawns the bullet impact particles according to the provided data from the event. </summary>
    public void SpawnBulletParticles(SpawnBulletImpactParticlesEvent eventData)
    {
        GpuParticles3D instantiatedParticlesScene = eventData.bulletImpactParticlesScene?.Instantiate<GpuParticles3D>();
        GetTree().Root.AddChild(instantiatedParticlesScene);
        instantiatedParticlesScene.Emitting = true;
        instantiatedParticlesScene.LookAtFromPosition(eventData.impactPoint, eventData.impactPoint + eventData.impactNormal, Vector3.Up);


    }
}
