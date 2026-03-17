using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using static UnityEngine.GraphicsBuffer;

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

    private void ApplyOutcome()
    {
        // kill the target
        // apply psychic soothe if successful
        // spawn an anima seed/pearl (need texture)
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref invokerRole, "invokerRole");
        Scribe_Defs.Look(ref targetRole, "targetRole");
    }
}
