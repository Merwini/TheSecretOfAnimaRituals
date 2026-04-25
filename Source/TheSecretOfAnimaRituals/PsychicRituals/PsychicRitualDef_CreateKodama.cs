using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using UnityEngine;

namespace tsoa.rituals;

public class PsychicRitualDef_CreateKodama : PsychicRitualDef_Unlocked
{
    private static Dictionary<WorkTypeDef, PawnKindDef> dict = new Dictionary<WorkTypeDef, PawnKindDef>()
    {
        { WorkTypeDefOf.Hauling, TSOAR_DefOf.TSOA_KodamaHaulerKind },
        { WorkTypeDefOf.Cleaning, TSOAR_DefOf.TSOA_KodamaCleanerKind },
        //{ WorkTypeDefOf.PlantCutting, TSOAR_DefOf.TSOA_KodamaCutterKind },
        //{ WorkTypeDefOf.Construction, TSOAR_DefOf.TSOA_KodamaRepairerKind }
    };

    // TODO
    public override List<string> FloatMenuOptionStrings
    {
        get
        {
            return null;
        }
    }

    public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
        list.Add(new PsychicRitualToil_CreateKodama(InvokerRole));
        return list;
    }
    public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
    {
        int max = assignments.AssignedPawnCount;
        int average = Mathf.FloorToInt(max * qualityRange.min);
        return outcomeDescription.Formatted(max, average);
    }
}
