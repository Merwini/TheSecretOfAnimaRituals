using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace tsoa.rituals;

public class PsychicRitualToil_CreateKodama : PsychicRitualToil_AnimaAffinity
{
    PsychicRitualRoleDef invokerRole;

    public PsychicRitualToil_CreateKodama()
    {
    }

    public PsychicRitualToil_CreateKodama(PsychicRitualRoleDef invokerRole)
    {
        this.invokerRole = invokerRole;
    }

    public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        base.Start(psychicRitual, parent);

        Pawn invoker = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
        int kodamaCount = 1;
        for (int i = 0; i < psychicRitual.assignments.AssignedPawnCount - 1; i++)
        {
            if (Rand.Chance(psychicRitual.PowerPercent))
            {
                kodamaCount++;
            }
        }

        if (invoker != null)
        {
            ApplyOutcome(psychicRitual, invoker, kodamaCount);
        }
    }

    private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, int kodamaCount)
    {
        for (int i = 0; i < kodamaCount; i++)
        {
            Pawn kodama = PawnGenerator.GeneratePawn(TSOAR_DefOf.TSOA_KodamaCleanerKind, Faction.OfPlayer);
            GenSpawn.Spawn(kodama, invoker.Position, invoker.Map);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref invokerRole, "invokerRole");
    }
}
