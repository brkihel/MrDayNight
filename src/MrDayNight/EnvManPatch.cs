using HarmonyLib;

namespace MrDayNight
{
    /// <summary>
    /// O ciclo do Valheim nao tem um "tamanho da noite". Ele tem um tamanho
    /// total (EnvMan.m_dayLengthSec) e uma proporcao fixa, escondida em
    /// RescaleDayFraction: a fracao crua 0.15..0.85 e remapeada para 0.25..0.75,
    /// e CalculateDay/CalculateNight comparam justamente contra 0.25 e 0.75.
    /// Resultado no jogo base: dia = 70% do ciclo, noite = 30%, sempre.
    ///
    /// Para controlar os dois de verdade, mexemos nos limites do remapeamento.
    /// A noite fica centrada na virada do ciclo (meia-noite), metade no fim e
    /// metade no comeco — que e como o jogo ja se comporta.
    /// </summary>
    internal static class Ciclo
    {
        /// <summary>Inicio da manha, em fracao crua. Vanilla: 0.15.</summary>
        internal static float Amanhecer => MrDayNight.FracaoNoite * 0.5f;

        /// <summary>Fim da tarde, em fracao crua. Vanilla: 0.85.</summary>
        internal static float Anoitecer => 1f - Amanhecer;
    }

    [HarmonyPatch(typeof(EnvMan), "Awake")]
    internal static class EnvMan_Awake_Patch
    {
        // Prioridade baixa de proposito: se outro mod (ZenWorldSettings, por
        // exemplo) tambem escrever m_dayLengthSec no Awake, queremos escrever
        // por ultimo.
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(EnvMan __instance)
        {
            if (__instance == null) return;
            MrDayNight.AplicarCiclo(__instance, "EnvMan.Awake");
        }
    }

    /// <summary>
    /// Substitui o remapeamento do jogo. O contrato de saida e o mesmo —
    /// 0.25..0.75 e dia, fora disso e noite — entao CalculateDay,
    /// CalculateNight, CalculateAfternoon e a iluminacao continuam funcionando
    /// sem precisar de patch nenhum.
    /// </summary>
    [HarmonyPatch(typeof(EnvMan), "RescaleDayFraction")]
    internal static class EnvMan_RescaleDayFraction_Patch
    {
        private static bool Prefix(float fraction, ref float __result)
        {
            if (!MrDayNight.Ativo) return true;   // deixa o jogo fazer do jeito dele

            float baixo = Ciclo.Amanhecer;
            float alto = Ciclo.Anoitecer;

            if (fraction < baixo)
                __result = fraction / baixo * 0.25f;              // madrugada
            else if (fraction > alto)
                __result = 0.75f + (fraction - alto) / baixo * 0.25f;  // noite
            else
                __result = 0.25f + (fraction - baixo) / (alto - baixo) * 0.5f;  // dia

            return false;
        }
    }

    /// <summary>
    /// Dormir leva ao inicio da manha, que o jogo calcula com o 0.15 fixo. Sem
    /// este patch, quem dormisse acordaria fora do amanhecer configurado.
    /// </summary>
    [HarmonyPatch(typeof(EnvMan), "GetMorningStartSec")]
    internal static class EnvMan_GetMorningStartSec_Patch
    {
        private static bool Prefix(EnvMan __instance, int day, ref double __result)
        {
            if (!MrDayNight.Ativo) return true;
            long ciclo = __instance.m_dayLengthSec;
            __result = (double)day * ciclo + ciclo * Ciclo.Amanhecer;
            return false;
        }
    }
}
