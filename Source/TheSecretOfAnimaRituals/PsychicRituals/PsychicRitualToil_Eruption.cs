using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace tsoa.rituals;

public class PsychicRitualToil_Eruption : PsychicRitualToil
{
    private PsychicRitualRoleDef invokerRole;
    private ThingDef chosenOreDef;

    public PsychicRitualToil_Eruption()
    {
    }

    public PsychicRitualToil_Eruption(PsychicRitualRoleDef invokerRole, ThingDef chosenOreDef)
    {
        this.invokerRole = invokerRole;
        this.chosenOreDef = chosenOreDef;
    }

    public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        base.Start(psychicRitual, parent);

        Pawn invoker = psychicRitual.assignments.FirstAssignedPawn(invokerRole);

        bool success = chosenOreDef == null
            ? Rand.Chance(((PsychicRitualDef_Eruption)psychicRitual.def).successCurveAnyOre.Evaluate(psychicRitual.PowerPercent))
            : Rand.Chance(((PsychicRitualDef_Eruption)psychicRitual.def).successCurve.Evaluate(psychicRitual.PowerPercent));

        psychicRitual.ReleaseAllPawnsAndBuildings();

        if (invoker != null)
        {
            ApplyOutcome(psychicRitual, invoker, success);
        }
    }

    private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, bool success)
    {
        if (chosenOreDef == null)
        {
            List<ThingDef> potentialOres = DefDatabase<ThingDef>.AllDefs.Where(td => td.building != null && td.building.isResourceRock).ToList();
            chosenOreDef = potentialOres.RandomElementByWeight(td => td.building.mineableScatterCommonality);
        }

        int oreAmount = chosenOreDef.building.mineableScatterLumpSizeRange.RandomInRange;
        int oreSqrt = Mathf.RoundToInt((float)Math.Sqrt(oreAmount));

        Map map = invoker.Map;

        IntVec3 chosenCell = ChooseCell(map, oreSqrt + 1);

        List<Thing> spawnedOres = SpawnOreVein(oreAmount, map, chosenCell);
        SpawnFilthAround(spawnedOres);

        LetterDef textLetterDef;
        TaggedString text;

        if (!success)
        {
            textLetterDef = LetterDefOf.NegativeEvent;
            text = "TSOA_EruptionFailure".Translate(invoker, psychicRitual.def.Named("RITUAL"));
            SpawnInfestation(invoker, map, chosenCell);
        }
        else
        {
            textLetterDef = LetterDefOf.PositiveEvent;
            text = "TSOA_EruptionSuccess".Translate(invoker, psychicRitual.def.Named("RITUAL"));
        }

        Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, textLetterDef, lookTargets: new LookTargets(chosenCell, map));
    }

    private void SpawnInfestation(Pawn invoker, Map map, IntVec3 chosenCell)
    {
        IntVec3 intVec = CellFinder.FindNoWipeSpawnLocNear(chosenCell, map, ThingDefOf.TunnelHiveSpawner, Rot4.North, 2, (IntVec3 x) => x.Walkable(map) && x.GetFirstThing(map, chosenOreDef) == null && x.GetFirstThing(map, ThingDefOf.Hive) == null && x.GetFirstThing(map, ThingDefOf.TunnelHiveSpawner) == null);
        TunnelHiveSpawner tunnelHiveSpawner = (TunnelHiveSpawner)ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner);
        tunnelHiveSpawner.spawnHive = false;
        tunnelHiveSpawner.insectsPoints = Mathf.Clamp(StorytellerUtility.DefaultThreatPointsNow(invoker.Map) * Rand.Range(0.3f, 0.6f), 200f, 1000f);
        tunnelHiveSpawner.spawnedByInfestationThingComp = true;
        GenSpawn.Spawn(tunnelHiveSpawner, intVec, map, WipeMode.FullRefund);
    }

    private List<Thing> SpawnOreVein(int oreAmount, Map map, IntVec3 chosenCell)
    {
        List<Thing> spawnedOres = new List<Thing>();
        foreach (IntVec3 cell in GridShapeMaker.IrregularLump(chosenCell, map, oreAmount, Validator))
        {
            spawnedOres.Add(GenSpawn.Spawn(chosenOreDef, cell, map));
        }
        return spawnedOres;
    }

    private void SpawnFilthAround(List<Thing> spawnedOres)
    {
        foreach (Thing ore in spawnedOres)
        {
            for (int i = 0; i < 4; i++)
            {
                IntVec3 adjacentCell = ore.Position + GenAdj.CardinalDirections[i];
                if (adjacentCell.InBounds(ore.Map) && Rand.Chance(0.5f))
                {
                    FilthMaker.TryMakeFilth(adjacentCell, ore.Map, ThingDefOf.Filth_RubbleRock);
                }
            }
        }
    }

    private IntVec3 ChooseCell(Map map, int size)
    {
        IntVec3 randomCell = CellFinder.RandomCell(map); 
        if (DropCellFinder.TryFindDropSpotNear(randomCell, map, out IntVec3 result, true, true, false, new IntVec2(size, size), false))
        {
            return result;
        }
        else
        {
            return randomCell;
        }
    }

    bool Validator(IntVec3 cell)
    {
        // TODO
        return true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref invokerRole, "invokerRole");
        Scribe_Defs.Look(ref chosenOreDef, "chosenOreDef");
    }
}