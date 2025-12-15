using BrilliantSkies.Common.Controls;
using BrilliantSkies.Constructs.Blocks.Sets.Unsorted.ModulesForPropulsionAndControl;
using HarmonyLib;
using UnityEngine;

namespace FtDSharp
{
    // maybe split into separate mod later
    [HarmonyPatch]
    public static class FixThrustBalancingPatch
    {
        /// <summary> Postfix patch that multiplies the thrust vector by the speed factor. </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StandardPropulsionModule), nameof(StandardPropulsionModule.ThrusterThrustVector), MethodType.Getter)]
        public static void Postfix_StandardPropulsionModule_ThrusterThrustVector(StandardPropulsionModule __instance, ref Vector3 __result)
        {
            if (__result.sqrMagnitude < 0.0001f)
                return;

            IControlUser user = __instance.User;
            if (user == null)
                return;

            CommonPropulsionBlock? propBlock = user as CommonPropulsionBlock;
            if (propBlock == null)
                return;

            float topSpeed = propBlock.TopSpeed;
            if (float.IsInfinity(topSpeed))
                return;

            IMainConstructBlock mainConstruct = propBlock.MainConstruct;
            if (mainConstruct == null)
                return;

            Vector3 localDirection = user.LocalInHullPropulsionActualForwards;

            Vector3 worldThrustDirection = mainConstruct.SafeLocalDirectionToGlobalDirection(localDirection);

            float speedFactor = GetThrustScaleForSpeed(mainConstruct, worldThrustDirection, topSpeed);

            __result *= speedFactor;
        }

        private static float GetThrustScaleForSpeed(IMainConstructBlock mainConstruct, Vector3 worldThrustDirection, float topSpeed)
        {
            float velocityInDirection = mainConstruct.PartPhysicsRestricted.iVelocities.VelocityInParticularDirection(worldThrustDirection);

            velocityInDirection = Mathf.Max(velocityInDirection, 1f);

            return PropulsionConstants.GetSpeedLimitForceFactor(velocityInDirection, topSpeed);
        }
    }
}