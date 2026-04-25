using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace tsoa.rituals;

public class JobGiver_KodamaClean : ThinkNode_JobGiver
{
    private static readonly WorkGiver_CleanFilth Worker = new WorkGiver_CleanFilth();

    public override Job TryGiveJob(Pawn pawn)
    {
        Map map = pawn.Map;

        if (pawn.Map == null || pawn.Downed)
        {
            return null;
        }

        Predicate<Thing> validator = thing =>
        {
            if (thing == null || !thing.Spawned || thing.Map != pawn.Map)
                return false;

            IntVec3 cell = thing.Position;

            if (!map.areaManager.Home[cell])
            {
                return false;
            }

            if (cell.Fogged(map))
                return false;

            if (cell.IsForbidden(pawn))
                return false;

            if (!pawn.CanReserve(thing))
                return false;

            if (!pawn.CanReach(thing, PathEndMode.Touch, Danger.Some))
                return false;

            return true;
        };

        Thing target = GenClosest.ClosestThingReachable(
            pawn.Position,
            map,
            ThingRequest.ForGroup(ThingRequestGroup.Filth),
            PathEndMode.Touch,
            TraverseParms.For(pawn, Danger.Some),
            validator: validator
        );

        if (target == null)
        {
            return null;
        }

        return Worker.JobOnThing(pawn, target);
    }
}