using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace tsoa.rituals
{
    public class PsychicRitualDef_Unlocked : PsychicRitualDef_InvocationCircle
    {
        public List<ThingDef> ritualFocuses; // List of allowed ritual focuses for this ritual
        public Thing ritualFocus; // The actual focus used in the ritual instance

        public bool targetsCell;
        public IntVec3 targetCell;

        public bool targetsPawn;
        public Pawn targetPawn;

        public ThingDef targetsThingOfDef;
        public Thing targetThing;


        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if (ritualFocuses.NullOrEmpty())
            {
                ritualFocuses = new List<ThingDef>();
                Log.Error($"Config error in {defName} from {modContentPack.Name}. Using class PsychicRitualDef_LocationUnlocked but has no list of ritual focuses.");
            }

            if ((targetsCell && targetsPawn) || (targetsCell && targetsThingOfDef != null) || (targetsPawn && targetsThingOfDef != null))
            {
                Log.Error($"Config error in {defName} from {modContentPack.Name}. Using class PsychicRitualDef_LocationUnlocked but has multiple target types set.");
            }
        }
    }
}
