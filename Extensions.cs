

using Godot;

public static class Extensions
{
    /// <summary> Converts a desired jump height into the needed velocity to achieve that height based on a supplied gravity magnitude</summary>
    /// <param name="desiredHeight"> The desired height to jump to</param>
    /// <param name="risingGravityMagnitude"> The magnitude of the gravity force acting on the player while the player rises from their jump</param>
    /// <returns> The velocity needed to achieve the desired height</returns>
    public static float ToJumpVelocity(this float desiredHeight, float risingGravityMagnitude)
    {
        return Mathf.Sqrt(2 * risingGravityMagnitude * desiredHeight);
    }

    /// <summary> Flattens a vector by setting its Y component to 0</summary>
    /// <param name="vector3"> The vector to flatten</param>
    /// <returns> The flattened vector</returns>
    public static Vector3 FlattenVector(this Vector3 vector3)
    {
        return new Vector3(vector3.X, 0, vector3.Z);
    }
}
