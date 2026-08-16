

using Godot;
using System.Collections.Generic;

/// <summary> A collection of useful extension methods for various types</summary>
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
    /// <returns> The original vector with its Y component set to 0</returns>
    public static Vector3 FlattenVector(this Vector3 vector3)
    {
        return new Vector3(vector3.X, 0, vector3.Z);
    }


    public static T FindChildOfType<T>(this Node startingSearchNode) where T : class
    {
        foreach (Node currentChildNode in startingSearchNode.GetChildren())
        {
            if (currentChildNode is T)
            {
                return currentChildNode as T;
            }
            T resultFromDeeperSearch = FindChildOfType<T>(currentChildNode);
            if (resultFromDeeperSearch != null)
            {
                return resultFromDeeperSearch;
            }
        }

        return null;
    }


    public static List<T> FindAllChildrenOfType<T>(this Node startingSearchNode) where T : class
    {
        List<T> results = new List<T>();
        foreach (Node currentChildNode in startingSearchNode.GetChildren())
        {
            if (currentChildNode is T)
            {
                results.Add(currentChildNode as T);
            }
            results.AddRange(FindAllChildrenOfType<T>(currentChildNode));

        }
        return results;
    }

    /// <summary> Returns the collision point if the raycast is colliding, otherwise returns the end point of the ray cast. </summary>
    public static Vector3 GetCollisionPointOrEndPoint(this RayCast3D rayCast)
    {
        return rayCast.IsColliding() ? rayCast.GetCollisionPoint() : rayCast.GlobalPosition + (rayCast.GlobalBasis * rayCast.TargetPosition);
    }
}
