using BrilliantSkies.Ftd.Missiles.Components;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Base class for missile part facades.
    /// Wraps a MissileComponent and provides common functionality.
    /// </summary>
    internal class MissilePartFacadeBase : IMissilePart
    {
        private readonly MissileComponent _component;

        public MissilePartFacadeBase(MissileComponent component)
        {
            _component = component;
        }

        /// <inheritdoc />
        public string PartType => _component.GetType().Name;

        /// <summary>
        /// Gets the underlying MissileComponent.
        /// </summary>
        protected MissileComponent Component => _component;
    }
}
