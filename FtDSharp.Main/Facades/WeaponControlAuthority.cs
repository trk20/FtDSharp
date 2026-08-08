using System.Collections.Generic;
using BrilliantSkies.Ai;
using BrilliantSkies.Blocks.Ai.WeaponControl;
using BrilliantSkies.Blocks.Ai.WeaponControl.Failsafe;
using BrilliantSkies.FromTheDepths.Game.UserInterfaces;
using FtDSharp.Helpers;
using HarmonyLib;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Resolves which alive LWC controls a weapon and reads failsafe / AI firing state.
    /// </summary>
    internal static class WeaponControlAuthority
    {
        /// <summary>
        /// Aim priority used when a script aims a weapon. Higher than any normal LWC priority.
        /// </summary>
        private const int ScriptAimPriority = int.MaxValue;

        private static readonly AccessTools.FieldRef<AiWeaponControlBlock, IFailsafeBlock?>? _failsafeField =
            AccessTools.FieldRefAccess<AiWeaponControlBlock, IFailsafeBlock?>("failsafe");

        private static MainConstruct? _cacheConstruct;
        private static FrameCache<Dictionary<ConstructableWeapon, AiWeaponControlBlock>>? _ownershipCache;

        /// <summary>
        /// True when an alive LWC lists this weapon in its native Weapons collection.
        /// </summary>
        public static bool HasControllingLwc(ConstructableWeapon weapon) => TryGetControllingLwc(weapon, out _);

        /// <summary>
        /// Finds the highest-priority alive LWC that claims this weapon.
        /// Hull LWCs often list only the turret ActiveBlock, not guns on that turret.
        /// Mounted weapons inherit the LWC of a claimed ancestor ActiveBlock up the subconstruct chain.
        /// </summary>
        public static bool TryGetControllingLwc(ConstructableWeapon weapon, out AiWeaponControlBlock? lwc)
        {
            lwc = null;
            if (weapon == null || !weapon.IsAlive)
                return false;

            if (weapon.MainConstruct is not MainConstruct main)
                return false;

            Dictionary<ConstructableWeapon, AiWeaponControlBlock> map = GetOwnershipMap(main);
            if (TryGetAliveController(map, weapon, out lwc))
                return true;

            // Same rule as ConstructableWeapon.CheckStatus: a mount is controlled if its related
            // turret is on an LWC (hull LWCs often list only the turret ActiveBlock).
            Turrets? relatedTurret = weapon.GetRelatedTurret();
            if (relatedTurret != null
                && relatedTurret != weapon
                && TryGetAliveController(map, relatedTurret, out lwc))
            {
                return true;
            }

            // Inherit from claimed turret (or other ActiveBlock) up the subconstruct chain.
            IAllConstructBlock? construct = weapon.GetConstructableOrSubConstructable();
            while (construct is SubConstruct sub)
            {
                if (sub.ActiveBlock is ConstructableWeapon claimable
                    && claimable != weapon
                    && TryGetAliveController(map, claimable, out lwc))
                {
                    return true;
                }

                construct = sub.Parent;
            }

            return false;
        }

        private static bool TryGetAliveController(
            Dictionary<ConstructableWeapon, AiWeaponControlBlock> map,
            ConstructableWeapon weapon,
            out AiWeaponControlBlock? lwc)
        {
            lwc = null;
            if (!map.TryGetValue(weapon, out AiWeaponControlBlock? controller))
                return false;

            if (controller == null || !controller.IsAlive)
                return false;

            lwc = controller;
            return true;
        }

        /// <summary>
        /// True when RespectAiFiring should allow fire for this LWC.
        /// Unlinked LWCs do not force a hold (mirrors FixedFire's linked+Off check).
        /// </summary>
        public static bool IsAiFiringAllowed(AiWeaponControlBlock lwc) => lwc != null && lwc.IsAlive && (!lwc.LinkedUp || !lwc.Node.IsInFiringMode(FiringType.Off));

        /// <summary>
        /// Failsafe attached to the LWC, or null if none / dead.
        /// </summary>
        public static ControllerFailsafe? GetFailsafe(AiWeaponControlBlock lwc)
        {
            if (lwc == null || _failsafeField == null)
                return null;

            IFailsafeBlock? block = _failsafeField(lwc);
            return block == null || !block.IsAlive ? null : block.Failsafe;
        }

        /// <summary>
        /// Whether fire is allowed under the given options (LWC required; optional AI firing).
        /// </summary>
        public static bool AllowsFire(ConstructableWeapon weapon, FireOptions options) => TryGetControllingLwc(weapon, out AiWeaponControlBlock? lwc) && lwc != null && (!options.RespectAiFiring || IsAiFiringAllowed(lwc));

        /// <summary>
        /// Marks the weapon as independently controlled so the LWC skips aim and fire
        /// (same path as player manual control via <c>IsBeingControlled</c>).
        /// Also records a high aim priority so lower-priority LWC aim orders lose.
        /// Parent turret ActiveBlocks are claimed too so the LWC does not keep driving the mount.
        /// </summary>
        public static void ClaimScriptControl(ConstructableWeapon weapon)
        {
            if (weapon == null || !weapon.IsAlive)
                return;

            ClaimOne(weapon);

            IAllConstructBlock? construct = weapon.GetConstructableOrSubConstructable();
            while (construct is SubConstruct sub)
            {
                if (sub.ActiveBlock is ConstructableWeapon parentWeapon
                    && parentWeapon.IsAlive
                    && parentWeapon != weapon)
                {
                    ClaimOne(parentWeapon);
                }

                construct = sub.Parent;
            }
        }

        private static void ClaimOne(ConstructableWeapon weapon)
        {
            weapon.ManualControlData.TimeWhenIndependentlyControlled.Now();
            weapon.InformAimedWithPriority(ScriptAimPriority);
        }

        private static Dictionary<ConstructableWeapon, AiWeaponControlBlock> GetOwnershipMap(MainConstruct main)
        {
            if (_ownershipCache == null || !ReferenceEquals(_cacheConstruct, main))
            {
                _cacheConstruct = main;
                _ownershipCache = new FrameCache<Dictionary<ConstructableWeapon, AiWeaponControlBlock>>(
                    () => BuildOwnershipMap(main),
                    () => main);
            }

            return _ownershipCache.Value;
        }

        private static Dictionary<ConstructableWeapon, AiWeaponControlBlock> BuildOwnershipMap(MainConstruct main)
        {
            var map = new Dictionary<ConstructableWeapon, AiWeaponControlBlock>();
            BlockStore<AiWeaponControlBlock>? store = main.iBlockTypeStorage?.LocalWeaponControllerStore;
            if (store?.Blocks == null)
                return map;

            foreach (AiWeaponControlBlock? lwc in store.Blocks)
            {
                if (lwc == null || !lwc.IsAlive || lwc.Weapons == null)
                    continue;

                var count = lwc.Weapons.Count;
                for (var i = 0; i < count; i++)
                {
                    ConstructableWeapon weapon = lwc.Weapons[i];
                    if (weapon == null || !weapon.IsAlive)
                        continue;

                    if (!map.TryGetValue(weapon, out AiWeaponControlBlock? existing))
                    {
                        map[weapon] = lwc;
                        continue;
                    }

                    if (lwc.Priority > existing.Priority)
                        map[weapon] = lwc;
                }
            }

            return map;
        }
    }
}
