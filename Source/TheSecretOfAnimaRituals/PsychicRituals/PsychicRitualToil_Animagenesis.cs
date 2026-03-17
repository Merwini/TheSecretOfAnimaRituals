using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using static UnityEngine.GraphicsBuffer;
using static Verse.PatchOperation;

namespace tsoa.rituals;

public class PsychicRitualToil_Animagenesis : PsychicRitualToil_AnimaAffinity
{
    PsychicRitualRoleDef invokerRole;
    PsychicRitualRoleDef targetRole;

    public PsychicRitualToil_Animagenesis()
    {
    }

    public PsychicRitualToil_Animagenesis(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole)
    {
        this.invokerRole = invokerRole;
        this.targetRole = targetRole;
    }

    public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        base.Start(psychicRitual, parent);

        bool success = Rand.Chance(((PsychicRitualDef_Animagenesis)psychicRitual.def).successCurve.Evaluate(psychicRitual.PowerPercent));

        Pawn invoker = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
        Pawn target = psychicRitual.assignments.FirstAssignedPawn(targetRole);

        if (invoker != null && target != null)
        {
            ApplyOutcome(psychicRitual, success, invoker, target);
        }
    }

    private void ApplyOutcome(PsychicRitual psychicRitual, bool success, Pawn invoker, Pawn target)
    {
        IntVec3 cell = target.Position;

        ExecutionUtility.DoExecutionByCut(invoker, target);

        if (success)
        {
            // Do psychic soothe
        }

        GenSpawn.Spawn(ThingDef.Named("TSOA_AnimaPearl"), cell, psychicRitual.Map);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref invokerRole, "invokerRole");
        Scribe_Defs.Look(ref targetRole, "targetRole");
    }
}
