using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;
using RimWorld;

namespace tsoa.rituals;

public class PsychicRitualToil_AnimaAffinity : PsychicRitualToil
{
    public override void End(PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
    {
        if (success && psychicRitual.def is PsychicRitualDef_Unlocked unlocked)
        {
            IEnumerable<Pawn> invokers = psychicRitual.assignments.AssignedPawns(unlocked.InvokerRole);
            foreach (Pawn invoker in invokers)
            {
                AddOrUpdateAffinityHediff(invoker, unlocked.invokerAffinity);
            }

            IEnumerable<Pawn> targets = psychicRitual.assignments.AssignedPawns(unlocked.TargetRole);
            foreach (Pawn target in targets)
            {
                AddOrUpdateAffinityHediff(target, unlocked.targetAffinity);
            }

            IEnumerable<Pawn> chanters = psychicRitual.assignments.AssignedPawns(unlocked.ChanterRole);
            foreach (Pawn chanter in chanters)
            {
                AddOrUpdateAffinityHediff(chanter, unlocked.chanterAffinity);
            }
        }

        base.End(psychicRitual, parent, success);
    }

    public void AddOrUpdateAffinityHediff(Pawn pawn, float amount)
    {
        Hediff_AnimaAffinity hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(TSOAR_DefOf.TSOA_AnimaAffinityHediff) as Hediff_AnimaAffinity;
        if (hediff == null)
        {
            hediff = (Hediff_AnimaAffinity)HediffMaker.MakeHediff(TSOAR_DefOf.TSOA_AnimaAffinityHediff, pawn);
            pawn.health.AddHediff(hediff);
        }
        hediff.AddAffinity(amount);
    }
}
