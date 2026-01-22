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
    public class PsychicRitualDef_Renewal : PsychicRitualDef_Unlocked
    {
        SimpleCurve successCurve;

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
            list.Add(new PsychicRitualToil_Renewal(InvokerRole, TargetRole));
            return list;
        }

        public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
        {
            foreach (string item in base.BlockingIssues(assignments, map))
            {
                yield return item;
            }
            Pawn target = assignments.FirstAssignedPawn(TargetRole);
            
            if ()
            {
                yield return "NotEnoughAnimals".Translate();
            }
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            return outcomeDescription.Formatted(successCurve.Evaluate(qualityRange.min).ToStringPercent());
        }

        public void ReturnMissingBodyPart(Pawn pawn)
        {
            // TODO
        }
    }
}
