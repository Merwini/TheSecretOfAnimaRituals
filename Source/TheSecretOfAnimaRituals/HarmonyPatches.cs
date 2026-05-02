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
        static MethodBase TargetMethod()
        {
            Type outerType = typeof(PsychicRitualGizmo);

            //var innerType = outerType
            //    .GetNestedTypes(BindingFlags.NonPublic)
            //    .First(t => t.Name.Contains("GetGizmos"));

            Type innerType = AccessTools.Inner(outerType, "<GetGizmos>d__0");

            //MethodInfo targetMethod = innerType
            //    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            //    .FirstOrDefault(m => m.Name.Contains("MoveNext"));

            MethodInfo targetMethod = AccessTools.Method(innerType, "MoveNext");

            return targetMethod;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            Type iteratorType = AccessTools.Inner(typeof(Verse.AI.Group.PsychicRitualGizmo), "<GetGizmos>d__0");

            FieldInfo targetField = AccessTools.Field(iteratorType, "target");

            MethodInfo filterMethod = typeof(HarmonyPatches).GetMethod("RitualTargetFilter", BindingFlags.Public | BindingFlags.Static);

            int orderByIndex = -1;
            int stlocIndex = -1;

            for (int i = 0; i < codes.Count; i++)
            {
                if (orderByIndex == -1 && codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("OrderBy"))
                {
                    orderByIndex = i;
                }

                if (orderByIndex != -1 && codes[i].opcode == OpCodes.Stloc_2)
                {
                    stlocIndex = i;
                    break;
                }
            }

            List<CodeInstruction> newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_2), // Load the unfiltered IOrderedEnumerable<PsychicRitualDef_InvocationCircle>
                new CodeInstruction(OpCodes.Ldarg_0), // Load "this"
                new CodeInstruction(OpCodes.Ldfld, targetField), // Load the target field from "this"
                new CodeInstruction(OpCodes.Call, filterMethod), // Call the RitualTargetFilter method
                new CodeInstruction(OpCodes.Stloc_2), // Store the filtered list IOrderedEnumerable<PsychicRitualDef_InvocationCircle>
            };

            codes.InsertRange(stlocIndex + 1, newCodes);

            return codes.AsEnumerable();
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

    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class Game_FinalizeInit_Patch
    {
        public static void Postfix()
        {
            CompBiHeatPusherRitualized.psychicRitualManager = Current.Game.GetComponent<GameComponent_PsychicRitualManager>();
        }
    }

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
            CompAffectedByGroupedFacilities comp = focus.TryGetComp<CompAffectedByGroupedFacilities>();
            if (comp == null)
            {
                return;
            }

            Dictionary<ThingDef, RitualQualityOffsetCount> dictionary = new Dictionary<ThingDef, RitualQualityOffsetCount>();
            List<Thing> groupedFacilitiesListForReading = focus.GetComp<CompAffectedByGroupedFacilities>().LinkedFacilities;
            for (int i = 0; i < groupedFacilitiesListForReading.Count; i++)
            {
                Thing thing = groupedFacilitiesListForReading[i];
                CompGroupedFacility compGroupedFacility = thing.TryGetComp<CompGroupedFacility>();
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
