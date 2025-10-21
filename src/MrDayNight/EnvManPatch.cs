using HarmonyLib;
using UnityEngine;

namespace MrDayNight
{
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    public static class EnvMan_Awake_Patch
    {
        static void Postfix(EnvMan __instance)
        {
            if (__instance == null)
                return;

            float day = MrDayNight.DayLength.Value;
            float night = MrDayNight.NightLength.Value;
            float total = day + night;

            __instance.m_dayLengthSec = (long)total;

            MrDayNight.Log.LogInfo(
                $"[MrDayNight] Ciclo personalizado aplicado: Dia={day}s, Noite={night}s, Total={total}s");
        }
    }
}