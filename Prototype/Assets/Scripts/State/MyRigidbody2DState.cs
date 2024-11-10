using FishNet.CodeGenerating;
using FishNet.Serializing;
using LiteNetLib.Utils;
using UnityEngine;

/// <summary>
///     Stripped down Rigidbody2D state with only the most basic information.
/// </summary>
[UseGlobalCustomSerializer]
[Preserve]
public struct BasicRigidbody2DState
{
    public Vector2 Position;
    public Vector2 Velocity;

    public BasicRigidbody2DState(Rigidbody2D rb)
    {
        Position = rb.transform.position;
        Velocity = rb.linearVelocity;
    }
}

[Preserve]
public static class BasicRigidbodyStateSerializers
{
    public static void WriteBasicRigidbody2DState(this Writer writer, BasicRigidbody2DState value)
    {
        writer.WriteVector2(value.Position);
        writer.WriteVector2(value.Velocity);
    }

    public static BasicRigidbody2DState ReadBasicRigidbody2DState(this Reader reader)
    {
        BasicRigidbody2DState state = new()
        {
            Position = reader.ReadVector2(),
            Velocity = reader.ReadVector2()
        };
        return state;
    }
}

[Preserve]
public static class BasicRigidbodyStateExtensions
{
    /// <summary>
    ///     Gets a Rigidbody2DState.
    /// </summary>
    public static BasicRigidbody2DState GetBasicState(this Rigidbody2D rb)
    {
        return new BasicRigidbody2DState(rb);
    }

    /// <summary>
    ///     Sets a state to a rigidbody.
    /// </summary>
    public static void SetBasicState(this Rigidbody2D rb, BasicRigidbody2DState state)
    {
        Transform t = rb.transform;
        t.position = state.Position;
        rb.linearVelocity = state.Velocity;
    }
}