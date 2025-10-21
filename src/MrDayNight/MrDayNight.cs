using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Extensions;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace MrDayNight
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [SynchronizationMode(AdminOnlyStrictness.IfOnServer)] // só sincroniza/trava se o servidor tiver Jötunn
    public class MrDayNight : BaseUnityPlugin
    {
        public const string PluginGUID = "genesisproj.mrdaynight";
        public const string PluginName = "MrDayNight";
        public const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log; // logger acessível de outros arquivos
        internal static ConfigEntry<float> DayLength;
        internal static ConfigEntry<float> NightLength;

        private Harmony harmony;

        private void Awake()
        {
            Log = Logger; // torna o logger acessível externamente

            // Cria configs sincronizadas automaticamente pelo Jötunn
            var dayRange = new AcceptableValueRange<float>(100f, 7200f);
            var nightRange = new AcceptableValueRange<float>(100f, 7200f);

            DayLength = Config.BindConfig(
                "Tempo", "DayLength",
                1200f,
                "Duração do dia em segundos (default = 1200)",
                synced: true,
                acceptableValues: dayRange
            );

            NightLength = Config.BindConfig(
                "Tempo", "NightLength",
                600f,
                "Duração da noite em segundos (default = 600)",
                synced: true,
                acceptableValues: nightRange
            );

            // Log inicial
            Log.LogInfo($"[{PluginName}] Inicializando v{PluginVersion}");

            // Hook de sincronização Jötunn
            SynchronizationManager.OnConfigurationSynchronized += (_, e) =>
            {
                if (e.InitialSynchronization)
                    Log.LogInfo("Configurações sincronizadas do servidor.");
                else
                    Log.LogInfo("Configurações atualizadas do servidor.");
            };

            // Aplica patches Harmony
            harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            Log.LogInfo("MrDayNight ativo e aguardando inicialização do EnvMan.");
        }

        private void OnDestroy() => harmony?.UnpatchSelf();
    }
}
