using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BrutalCompany.ManualPatches
{
    [HarmonyPatch(typeof(TimeOfDay), "Start")]
    internal class Patch_QuotaAjuster
    {
        static void Prefix(TimeOfDay __instance)
        {
            Plugin.mls.LogWarning("Changing quota variables in patch!");
            __instance.quotaVariables.startingQuota = 1000;
            __instance.quotaVariables.startingCredits = 250;
            __instance.quotaVariables.baseIncrease = 500;
            __instance.quotaVariables.randomizerMultiplier = 0;
            __instance.quotaVariables.deadlineDaysAmount = 10;
        }
    }
}
