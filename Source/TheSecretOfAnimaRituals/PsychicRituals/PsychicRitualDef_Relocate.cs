using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace tsoa.rituals;

public class PsychicRitualDef_Relocate : PsychicRitualDef_Unlocked
{
    public SimpleCurve relocateCurve;

    public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
    {
        List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
        list.Add(new PsychicRitualToil_Relocate(InvokerRole, targetCell, ritualFocus));

        return list;
    }

    public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
    {
        return outcomeDescription.Formatted(qualityRange.min.ToStringPercent());
    }
}