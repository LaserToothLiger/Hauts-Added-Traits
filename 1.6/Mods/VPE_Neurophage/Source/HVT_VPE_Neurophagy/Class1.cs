using HarmonyLib;
using HautsTraitsRoyalty;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using VPE_Neurophage;

namespace HVT_VPE_Neurophagy
{
    [StaticConstructorOnStartup]
    public class HVT_VPE_Neurophagy
    {
        private static readonly Type patchType = typeof(HVT_VPE_Neurophagy);
        static HVT_VPE_Neurophagy()
        {
            Harmony harmony = new Harmony(id: "rimworld.hautarche.HVT_VPEneurophage");
            MethodInfo methodInfo = typeof(Ability_MassNeurophagy).GetMethod("AffectAoE", BindingFlags.NonPublic | BindingFlags.Instance);
            harmony.Patch(methodInfo,
                          prefix: new HarmonyMethod(patchType, nameof(HVT_VPEN_ApexNeuorphagyPostfix)));
        }
        //while the single-target Neurophage psycasts can just be slapped with the AbilityExtension that forbids targeting woke pawns, ApexNeurophagy is an AoE and therefore needs this patch to avoid moving around woke traits.
        public static bool HVT_VPEN_ApexNeuorphagyPostfix(Ability_MassNeurophagy __instance, Pawn CasterPawn, IntVec3 targetPoint)
        {
            bool flag = false;
            List<Pawn> list = new List<Pawn>();
            foreach (Pawn pawn in CasterPawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn != CasterPawn && pawn.Position.InHorDistOf(targetPoint, __instance.def.radius) && !PsychicTraitAndGeneCheckUtility.IsAwakenedPsychic(pawn))
                {
                    bool flag3 = pawn.Faction != CasterPawn.Faction;
                    if (flag3)
                    {
                        flag = true;
                    }
                    list.Add(pawn);
                }
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Pawn pawn2 = list[i];
                bool flag4 = flag && pawn2.Faction == CasterPawn.Faction;
                if (!flag4)
                {
                    Ability_PsychicTheft.Linkophagy(pawn2, CasterPawn);
                }
            }
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(targetPoint, CasterPawn.Map, __instance.def.radius, true))
            {
                if (thing is Corpse corpse && corpse != null && corpse.InnerPawn != null)
                {
                    Ability_PsychicTheft.Linkophagy(corpse.InnerPawn, CasterPawn);
                }
            }
            return false;
        }
    }
}
