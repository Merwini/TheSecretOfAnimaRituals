using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace tsoa.rituals;

public class PsychicRitualDef_Eruption : PsychicRitualDef_Unlocked
{
    public SimpleCurve successCurve;
    public SimpleCurve successCurveAnyOre;
    public string anyOreOptionString;

    private ThingDef chosenOreDef;

    public override List<string> FloatMenuOptionStrings
    {
        get
        {
            List<string> options = new List<string>();
            options.Add(anyOreOptionString);

            if (advancedResearchProject.IsFinished)
            {
                foreach (var ore in DefDatabase<ThingDef>.AllDefs.Where(td => td.building != null && td.building.isResourceRock))
                {
                    options.Add(ore.label);
                }
            }

            return options;
        }
    }

    public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
    {
        List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
        list.Add(new PsychicRitualToil_Eruption(InvokerRole, chosenOreDef));
        return list;
    }

    public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
    {
        if (chosenOreDef == null) // i.e. "any ore" was selected
        {
            return outcomeDescription.Formatted(successCurveAnyOre.Evaluate(qualityRange.min).ToStringPercent());
        }
        return outcomeDescription.Formatted(successCurve.Evaluate(qualityRange.min).ToStringPercent());
    }

    public override void InitializeCast(Map map)
    {
        base.InitializeCast(map);

        chosenOreDef = ResolveOreDef(selectedFloatMenuOptionString);
    }

    private ThingDef ResolveOreDef(string chosen)
    {
        return DefDatabase<ThingDef>.AllDefs.FirstOrDefault(td => td.label == chosen);
    }
}