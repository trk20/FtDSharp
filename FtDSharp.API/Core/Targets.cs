
namespace FtDSharp
{

    public interface ITarget : IConstruct
    {
        // Error is only relevant for a target
        public float PositionError { get; }
        // Priority-relevant properties
        public float TotalFirePower { get; }
        public float ApsPower { get; }
        public float CramPower { get; }
        public float MissilePower { get; }
        public float LaserPower { get; }
        public float PacPower { get; }
        public float PlasmaPower { get; }
        public float FlamerPower { get; }
        public float SimpleCannonPower { get; }
        public float SimpleLaserPower { get; }
        public float MeleePower { get; }
        public float ArmorCostPercent { get; }
        public float PropulsionScore { get; }
        public float PowerScore { get; }
        public float AICount { get; }
    }

}