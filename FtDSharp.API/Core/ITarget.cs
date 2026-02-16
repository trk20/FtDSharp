namespace FtDSharp
{
    /// <summary>
    /// Represents an enemy target with detection error and firepower information.
    /// </summary>
    public interface ITarget : IConstruct
    {
        /// <summary>Detection error applied to position.</summary>
        float PositionError { get; }
        /// <summary>Total combined firepower score.</summary>
        float TotalFirePower { get; }
        /// <summary>APS firepower score.</summary>
        float ApsPower { get; }
        /// <summary>CRAM firepower score.</summary>
        float CramPower { get; }
        /// <summary>Missile firepower score.</summary>
        float MissilePower { get; }
        /// <summary>Laser firepower score.</summary>
        float LaserPower { get; }
        /// <summary>PAC firepower score.</summary>
        float PacPower { get; }
        /// <summary>Plasma firepower score.</summary>
        float PlasmaPower { get; }
        /// <summary>Flamer firepower score.</summary>
        float FlamerPower { get; }
        /// <summary>Simple cannon firepower score.</summary>
        float SimpleCannonPower { get; }
        /// <summary>Simple laser firepower score.</summary>
        float SimpleLaserPower { get; }
        /// <summary>Melee firepower score.</summary>
        float MeleePower { get; }
        /// <summary>Percent of cost spent on armor.</summary>
        float ArmorCostPercent { get; }
        /// <summary>Propulsion score for priority calculation.</summary>
        float PropulsionScore { get; }
        /// <summary>Power score for priority calculation.</summary>
        float PowerScore { get; }
        /// <summary>Number of AI mainframes on the target.</summary>
        float AICount { get; }
    }
}
