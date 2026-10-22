using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using tsoa.core;
using Verse;
using Verse.AI.Group;
using RimWorld;
using static RimWorld.PsychicRitualDef_InvocationCircle;
using HarmonyLib;

namespace tsoa.rituals;

[StaticConstructorOnStartup]
public class HarmonyPatches
{
    static HarmonyPatches()
    {
        Harmony harmony = new Harmony("tsoa.rituals");

        harmony.PatchAll();
    }


    // Plan:
    /* IOrderedEnumerable<PsychicRitualDef_InvocationCircle> orderedEnumerable = from ritualDef in VisibleRituals()
    orderby ritualDef.label
    select ritualDef;

    INSERT HELPER METHOD HERE to check if the PsychicRitualDef_InvocationCircle is actually a PsychicRitualDef_LocationUnlocked, if so, check the ritualFocuses against the target, don't return that ritual def if it doesn't match.

    foreach (PsychicRitualDef_InvocationCircle ritualDef2 in orderedEnumerable) */
    [HarmonyPatch(typeof(PsychicRitualGizmo))]
    public class PsychicRitualGizmo_GetGizmos_Patch
    {
        private static readonly MethodInfo moveNext =
        AccessTools.EnumeratorMoveNext(
            AccessTools.Method(
                typeof(PsychicRitualGizmo),
                nameof(PsychicRitualGizmo.GetGizmos)));

        static bool Prepare()
        {
            if (moveNext != null)
                return true;

            Log.Error("[TSOA] Could not find GetGizmos iterator; skipping ritual filter patch.");
            return false;
        }

        static MethodBase TargetMethod() => moveNext;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var codes = instructions.ToList();

            FieldInfo targetField = AccessTools.Field(__originalMethod.DeclaringType, "target");

            MethodInfo filterMethod = AccessTools.Method(typeof(HarmonyPatches), nameof(RitualTargetFilter));

            MethodInfo orderBy = typeof(Enumerable).GetMethods()
                .Single(m => m.Name == nameof(Enumerable.OrderBy)
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 2)
                .MakeGenericMethod(
                    typeof(PsychicRitualDef_InvocationCircle),
                    typeof(string));

            List<int> matches = Enumerable.Range(0, codes.Count)
                .Where(i => codes[i].Calls(orderBy))
                .ToList();

            if (targetField == null || targetField.IsStatic
                || targetField.FieldType != typeof(Thing)
                || filterMethod == null || matches.Count != 1)
            {
                Log.Error("[TSOA] GetGizmos IL did not match expectations; leaving it unchanged.");
                return codes;
            }

            codes.InsertRange(matches[0] + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0), // Load "this"
                new CodeInstruction(OpCodes.Ldfld, targetField), // Load the "target" field from "this"
                new CodeInstruction(OpCodes.Call, filterMethod) // Call the RitualTargetFilter method
            });

            return codes;
        }
    }
    public static IOrderedEnumerable<PsychicRitualDef_InvocationCircle> RitualTargetFilter(IOrderedEnumerable<PsychicRitualDef_InvocationCircle> unfiltered, Thing target)
    {
        bool isPsychicRitualSpot = target != null && target.def == TSOAR_DefOf.PsychicRitualSpot;

        return unfiltered
            .Where(ritualDef =>
            {
                PsychicRitualDef_Unlocked locationUnlocked = ritualDef as PsychicRitualDef_Unlocked;

                // Case: it's a psychic ritual spot and the ritual is NOT location unlocked, or is and the target is psychic ritual spot
                if (isPsychicRitualSpot)
                {
                    return locationUnlocked == null || locationUnlocked.ritualFocuses.Contains(TSOAR_DefOf.PsychicRitualSpot);
                }

                // Case: it's not a psychic ritual spot and the ritual is location unlocked and the ritual focus matches the target
                return locationUnlocked != null && locationUnlocked.ritualFocuses.Contains(target.def);
            })
            .OrderBy(rd => rd.label);
    }

    [HarmonyPatch(typeof(PsychicRitualGizmo), nameof(PsychicRitualGizmo.InitializePsychicRitual))]
    public static class PsychicRitualGizmo_InitializePsychicRitual_Prefix
    {
        public static bool Prefix(PsychicRitualDef_InvocationCircle psychicRitualDef, Thing target)
        {
            if (psychicRitualDef is not PsychicRitualDef_Unlocked unlockedRitual)
                return true;

            unlockedRitual.ritualFocus = target;

            Map map = Find.CurrentMap;

            if (unlockedRitual.targetsCell)
            {
                Find.Targeter.BeginTargeting(
                new TargetingParameters
                {
                    canTargetLocations = true,
                    canTargetBuildings = false,
                    canTargetPawns = false,
                    validator = t =>
                    {
                        IntVec3 cell = t.Cell;
                        return cell.InBounds(map) && unlockedRitual.ExtraCellValidator(cell, t.Map);
                    }
                },
                localTarget =>
                {
                    unlockedRitual.targetCell = localTarget.Cell;

                    OriginalActions();
                });

                return false;
            }
            else if (unlockedRitual.targetsPawn)
            {
                Find.Targeter.BeginTargeting(
                new TargetingParameters
                {
                    canTargetPawns = true,
                    canTargetBuildings = false,
                    validator = t =>
                        t.Thing is Pawn &&
                        unlockedRitual.ExtraThingValidator(t.Thing)
                },
                localTarget =>
                {
                    Pawn pawn = localTarget.Pawn;
                    if (pawn == null)
                        return;

                    unlockedRitual.targetPawn = pawn;

                    OriginalActions();
                });

                return false;
            }
            else if (unlockedRitual.targetsThingOfDef != null)
            {
                ThingDef wantedDef = unlockedRitual.targetsThingOfDef;

                Find.Targeter.BeginTargeting(
                    new TargetingParameters
                    {
                        canTargetPawns = false,
                        canTargetBuildings = true,
                        canTargetPlants = true,
                        canTargetItems = true,
                        validator = t =>
                            t.Thing != null &&
                            t.Thing.def == wantedDef &&
                            // Probably redundant
                            //t.Thing.Spawned &&
                            //t.Thing.Map == map && 
                            unlockedRitual.ExtraThingValidator(t.Thing)
                    },
                    localTarget =>
                    {
                        unlockedRitual.targetThing = localTarget.Thing;

                        OriginalActions();
                    });

                return false;
            }
            else if (unlockedRitual.gizmoMakesFloatMenu)
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();

                foreach (string item in unlockedRitual.FloatMenuOptionStrings)
                {
                    opts.Add(new FloatMenuOption(
                        item,
                        //() => unlockedRitual.HandleFloatMenuOption(item)));
                        () =>
                        {
                            unlockedRitual.selectedFloatMenuOptionString = item;

                            OriginalActions();
                        }
                        ));
                }

                Find.WindowStack.Add(new FloatMenu(opts));

                return false;
            }

            // implied else
            return true;

            void OriginalActions()
            {
                TargetInfo target2 = new TargetInfo(target);
                PsychicRitualRoleAssignments assignments = psychicRitualDef.BuildRoleAssignments(target2);
                PsychicRitualCandidatePool candidatePool = psychicRitualDef.FindCandidatePool();
                Map currentMap = Find.CurrentMap;
                psychicRitualDef.InitializeCast(currentMap);
                Find.WindowStack.Add(new Dialog_BeginPsychicRitual(psychicRitualDef, candidatePool, assignments, currentMap));
            }
        }
    }

    //[HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    //public static class Game_FinalizeInit_Patch
    //{
    //    public static void Postfix()
    //    {
    //        CompBiHeatPusherRitualized.psychicRitualManager = Current.Game.GetComponent<GameComponent_PsychicRitualManager>();
    //    }
    //}

    [HarmonyPatch(typeof(PsychicRitualDef_InvocationCircle), "CalculateMaxPower")]
    public static class PsychicRitualDef_InvocationCircle_CalculateMaxPower_Postfix
    {
        public static void Postfix(PsychicRitualRoleAssignments assignments, List<QualityFactor> powerFactorsOut, ref float power)
        {
            if (assignments.Target.Thing is ThingWithComps thing)
            {
                CalculateGroupedFacilityQualityOffset(powerFactorsOut, ref power, thing);
            }
        }

        private static void CalculateGroupedFacilityQualityOffset(List<QualityFactor> powerFactorsOut, ref float power, ThingWithComps focus)
        {
            CompAffectedByFacilities_Grouped comp = focus.TryGetComp<CompAffectedByFacilities_Grouped>();
            if (comp == null)
            {
                return;
            }

            Dictionary<ThingDef, RitualQualityOffsetCount> dictionary = new Dictionary<ThingDef, RitualQualityOffsetCount>();
            List<Thing> groupedFacilitiesListForReading = focus.GetComp<CompAffectedByFacilities_Grouped>().LinkedFacilitiesListForReading;
            for (int i = 0; i < groupedFacilitiesListForReading.Count; i++)
            {
                Thing thing = groupedFacilitiesListForReading[i];
                CompFacility_Grouped compGroupedFacility = thing.TryGetComp<CompFacility_Grouped>();
                if (compGroupedFacility?.StatOffsets == null || !compGroupedFacility.CanBeActive)
                {
                    Log.Message("continue due to no comp, no StatOffsets, or not active");
                    continue;
                }
                for (int j = 0; j < compGroupedFacility.StatOffsets.Count; j++)
                {
                    StatModifier statModifier = compGroupedFacility.StatOffsets[j];
                    if (statModifier.stat == StatDefOf.PsychicRitualQuality)
                    {
                        if (dictionary.TryGetValue(thing.def, out var value))
                        {
                            value.count++;
                            value.offset += statModifier.value;
                        }
                        else
                        {
                            dictionary.Add(thing.def, new RitualQualityOffsetCount(1, statModifier.value));
                        }
                    }
                }
            }
            foreach (KeyValuePair<ThingDef, RitualQualityOffsetCount> item in dictionary)
            {
                powerFactorsOut?.Add(new QualityFactor
                {
                    label = Find.ActiveLanguageWorker.Pluralize(item.Key.label).CapitalizeFirst(),
                    positive = true,
                    count = item.Value.count.ToString(),
                    quality = item.Value.offset,
                    toolTip = "PsychicRitualDef_InvocationCircle_QualityFactor_Increase_Tooltip".Translate().CapitalizeFirst().EndWithPeriod()
                });
                power += item.Value.offset;
            }
        }
    }
}
