using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace tsoa.rituals;

public class PsychicRitualDef_HungryGrave : PsychicRitualDef_Unlocked
{
    public SimpleCurve graveCurve;
    public float baseDurationHours;

    public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
    {
        List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
        list.Add(new PsychicRitualToil_HungryGrave(InvokerRole));
        return list;
    }
}
