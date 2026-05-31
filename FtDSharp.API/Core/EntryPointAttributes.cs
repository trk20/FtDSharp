using System;

namespace FtDSharp
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnPhysicsTickAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnStartAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnStopAttribute : Attribute
    {
    }
}