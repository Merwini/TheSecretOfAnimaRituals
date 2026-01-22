using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using static UnityEngine.GraphicsBuffer;

namespace tsoa.rituals
{
    public class PsychicRitualToil_Renewal : PsychicRitualToil
    {
        private PsychicRitualRoleDef invokerRole;
        private PsychicRitualRoleDef targetRole;

        public PsychicRitualToil_Renewal()
        {
        }

        public PsychicRitualToil_Renewal(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole)
        {
            this.invokerRole = invokerRole;
            this.targetRole = targetRole;
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            base.Start(psychicRitual, parent);
            Pawn invoker = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            Pawn target = psychicRitual.assignments.FirstAssignedPawn(targetRole);

            bool success = Rand.Chance(((PsychicRitualDef_Fever)psychicRitual.def).successCurve.Evaluate(psychicRitual.PowerPercent));

            if (invoker != null && target != null)
            {
                ApplyOutcome(psychicRitual, success, invoker, target);
            }
        }

        private void ApplyOutcome(PsychicRitual psychicRitual, bool success, Pawn invoker, Pawn targets)
        {

        }
    }
}
