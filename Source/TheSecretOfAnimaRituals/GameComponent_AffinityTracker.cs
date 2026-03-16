using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace tsoa.rituals;

public class GameComponent_AffinityTracker : GameComponent
{
    public Dictionary<Pawn, int> animaAffinityDict = new Dictionary<Pawn, int>();

    public GameComponent_AffinityTracker(Game game)
    {
    }

    public void UpdateAnimaAffinityFor(Pawn pawn, int amount)
    {
        if (!animaAffinityDict.TryGetValue(pawn, out var affinity))
        {
            animaAffinityDict[pawn] = 0;
        }

        affinity += amount;

        UpdateAnimaHediffFor(pawn, affinity);
    }

    private void UpdateAnimaHediffFor(Pawn pawn, int newAffinity)
    {
        Log.Warning($"Affinity for {pawn.Label}: {newAffinity}");
        // TODO make or update hediff
    }

    public override void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            CleanDictionary();
        }

        Scribe_Collections.Look(ref animaAffinityDict, "animaAffinityDict", LookMode.Reference, LookMode.Value);

        base.ExposeData();
    }

    // Dead/destroyed pawns shouldn't cause issues during gameplay, but we don't want to try saving them
    private void CleanDictionary()
    {
        List<Pawn> removalList = new List<Pawn>();
        foreach (var kvp in animaAffinityDict)
        {
            if (kvp.Key is not Pawn pawn || pawn.Destroyed)
            {
                removalList.Add(kvp.Key);
            }
        }

        foreach (var pawn in removalList)
        {
            animaAffinityDict.Remove(pawn);
        }
    }
}
