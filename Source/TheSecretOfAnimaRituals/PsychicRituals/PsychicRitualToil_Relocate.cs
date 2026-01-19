using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static UnityEngine.GraphicsBuffer;

namespace tsoa.rituals
{
    public class PsychicRitualToil_Relocate : PsychicRitualToil
    {
        private PsychicRitualRoleDef invokerRole;
        private IntVec3 targetCell;
        private Thing ritualFocus;

        public PsychicRitualToil_Relocate()
        {
        }

        public PsychicRitualToil_Relocate(PsychicRitualRoleDef invokerRole, IntVec3 targetCell, Thing ritualFocus)
        {
            this.invokerRole = invokerRole;
            this.targetCell = targetCell;
            this.ritualFocus = ritualFocus;
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            base.Start(psychicRitual, parent);
            Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            float rand = Rand.Range(0f, 1f);
            float failureDegree = rand - psychicRitual.PowerPercent;
            psychicRitual.ReleaseAllPawnsAndBuildings();
            if (pawn != null)
            {
                ApplyOutcome(psychicRitual, pawn, failureDegree);
            }
        }

        private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, float failureDegree)
        {
            IntVec3 destination;
            if (failureDegree <= 0)
            {
                destination = targetCell;
            }
            else
            {
                float failureDistance = failureDegree * 10f;
                // Find a random empty cell approximately failureDistance away from targetCell
            }
            EffecterDefOf.Skip_Entry.Spawn(ritualFocus.Position, ritualFocus.Map);
            EffecterDefOf.SkipExit.Spawn(destination, ritualFocus.Map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
        }
    }
}
