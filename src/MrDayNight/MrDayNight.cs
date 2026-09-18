using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Extensions;
using Jotunn.Managers;
using Jotunn.Utils;

namespace MrDayNight
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    // Dependencia fraca so para ordenar a carga: o ZenWorldSettings tambem
    // escreve EnvMan.m_dayLengthSec, e queremos entrar depois dele.
    [BepInDependency(GuidZenWorldSettings, BepInDependency.DependencyFlags.SoftDependency)]
    [SynchronizationMode(AdminOnlyStrictness.IfOnServer)]
    public class MrDayNight : BaseUnityPlugin
    {
        public const string PluginGUID = "genesisproj.mrdaynight";
        public const string PluginName = "MrDayNight";
        public const string PluginVersion = "2.0.0";

        internal const string GuidZenWorldSettings = "ZenDragon.ZenWorldSettings";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<float> DayLength;
        internal static ConfigEntry<float> NightLength;

        private Harmony harmony;

        internal static bool Ativo => Enabled != null && Enabled.Value;

        /// <summary>Ciclo completo em segundos: dia + noite.</summary>
        internal static float CicloTotal =>
            Math.Max(1f, DayLength.Value + NightLength.Value);

        /// <summary>Quanto do ciclo e noite, de 0 a 1. Vanilla: 0.30.</summary>
        internal static float FracaoNoite =>
            Mathf_Clamp(NightLength.Value / CicloTotal, 0.02f, 0.98f);

        // Clamp sem depender do UnityEngine so para isto.
        private static float Mathf_Clamp(float v, float min, float max) =>
            v < min ? min : (v > max ? max : v);

        private void Awake()
        {
            Log = Logger;

            Enabled = Config.BindConfig("Tempo", "Enabled", true,
                "Se desligado, o jogo volta ao ciclo original (dia 70%, noite 30%).",
                synced: true);

            // Os padroes reproduzem o vanilla exatamente: 1800s de ciclo,
            // divididos em 70% de dia e 30% de noite. Instalar o mod sem
            // configurar nada nao muda nada.
            DayLength = Config.BindConfig("Tempo", "DayLength", 1260f,
                "Duracao do dia em segundos. Vanilla: 1260 (70% de 1800).",
                synced: true, acceptableValues: new AcceptableValueRange<float>(10f, 7200f));

            NightLength = Config.BindConfig("Tempo", "NightLength", 540f,
                "Duracao da noite em segundos. Vanilla: 540 (30% de 1800).",
                synced: true, acceptableValues: new AcceptableValueRange<float>(10f, 7200f));

            Log.LogInfo($"[{PluginName}] v{PluginVersion} iniciando.");
            AvisarSobreOutrosMods();

            // Mexer no config em jogo deve valer na hora, sem reiniciar.
            Enabled.SettingChanged += (_, __) => Reaplicar("config alterada");
            DayLength.SettingChanged += (_, __) => Reaplicar("config alterada");
            NightLength.SettingChanged += (_, __) => Reaplicar("config alterada");

            SynchronizationManager.OnConfigurationSynchronized += (_, e) =>
                Reaplicar(e.InitialSynchronization
                    ? "configuracao recebida do servidor"
                    : "configuracao atualizada pelo servidor");

            harmony = new Harmony(PluginGUID);
            harmony.PatchAll();
        }

        /// <summary>
        /// Quem controla o ciclo precisa ficar claro no log — tanto para quem usa
        /// outro mod de mundo quanto para quem nao usa nenhum.
        /// </summary>
        private static void AvisarSobreOutrosMods()
        {
            if (Chainloader.PluginInfos.TryGetValue(GuidZenWorldSettings, out var zen))
            {
                Log.LogWarning(
                    $"{zen.Metadata.Name} {zen.Metadata.Version} tambem controla a duracao do dia " +
                    "(a opcao 'Day Length Seconds' dele). O MrDayNight assume o controle do ciclo " +
                    "e sobrescreve aquele valor — ajuste o ciclo aqui, nao la. " +
                    "Para devolver o controle, desligue 'Tempo.Enabled' nesta configuracao.");
            }
            else
            {
                Log.LogInfo("Nenhum outro mod de ciclo detectado; o MrDayNight esta no controle.");
            }
        }

        private static void Reaplicar(string motivo)
        {
            var env = EnvMan.instance;
            if (env == null) return;      // ainda nao ha mundo; o Awake do EnvMan cuida
            AplicarCiclo(env, motivo);
        }

        internal static void AplicarCiclo(EnvMan env, string motivo)
        {
            if (!Ativo)
            {
                Log.LogInfo($"Desligado ({motivo}); o ciclo original do jogo foi mantido.");
                return;
            }

            env.m_dayLengthSec = (long)CicloTotal;

            float amanhecer = Ciclo.Amanhecer;
            Log.LogInfo(
                $"Ciclo aplicado ({motivo}): dia {DayLength.Value:0}s + noite {NightLength.Value:0}s " +
                $"= {CicloTotal:0}s. Amanhece em {amanhecer:P1} do ciclo, anoitece em {Ciclo.Anoitecer:P1} " +
                $"(vanilla: 15.0% e 85.0%).");
        }

        private void OnDestroy() => harmony?.UnpatchSelf();
    }
}
