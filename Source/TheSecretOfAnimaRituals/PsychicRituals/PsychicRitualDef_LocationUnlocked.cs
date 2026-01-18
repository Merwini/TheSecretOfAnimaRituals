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
    public class PsychicRitualDef_LocationUnlocked : PsychicRitualDef_InvocationCircle
    {
        public List<ThingDef> ritualFocuses;

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if (ritualFocuses.NullOrEmpty())
            {
                ritualFocuses = new List<ThingDef>();
                Log.Error($"Config error in {defName} from {modContentPack.Name}. Using class PsychicRitualDef_LocationUnlocked but has no list of ritual focuses.");
            }
        }
    }
}
