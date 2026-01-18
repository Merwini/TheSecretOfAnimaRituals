using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace tsoa.rituals
{
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

                Type iteratorType = AccessTools.Inner(typeof(Verse.AI.Group.PsychicRitualGizmo),"<GetGizmos>d__0");

                FieldInfo targetField = AccessTools.Field(iteratorType, "target");

                MethodInfo filterMethod = typeof(HarmonyPatches).GetMethod("RitualTargetFilter", BindingFlags.Public | BindingFlags.Static);

                int orderByIndex = -1;
                int stlocIndex = -1;

                for (int i = 0; i < codes.Count; i++)
                {
                    if (orderByIndex == -1 && codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("OrderBy"))
                    {
                        orderByIndex = i;
                        Log.Message(codes[i].ToString());
                    }

                    if (orderByIndex != -1 && codes[i].opcode == OpCodes.Stloc_2)
                    {
                        stlocIndex = i;
                        Log.Message(codes[i].ToString());
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
                    PsychicRitualDef_LocationUnlocked locationUnlocked = ritualDef as PsychicRitualDef_LocationUnlocked;

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
    }
}
