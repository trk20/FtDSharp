namespace FtDSharp
{
    /// <summary>
    /// Represents a construct with identification and status information.
    /// </summary>
    public interface IConstruct : ITargetable
    {
        /// <summary>Unique identifier for the construct.</summary>
        int UniqueId { get; }
        /// <summary>The name of the construct.</summary>
        string Name { get; }
        /// <summary>Total volume of the construct.</summary>
        float Volume { get; }
        /// <summary>Number of living blocks on the construct.</summary>
        int AliveBlockCount { get; }
        /// <summary>Total number of blocks on the construct.</summary>
        int BlockCount { get; }
        /// <summary>Stability factor of the construct.</summary>
        float Stability { get; }
    }
}
