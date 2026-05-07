using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using tsoa.core;

namespace tsoa.rituals;

public class Building_RootGraveRitualized : Building_RootGrave
{
    private const float hungryGraveConsumeMult = 10f;

    public override float ConsumeRate
    {
        get
        {
            if (GameComponent_AnimaRitual.Instance.hungryGraveEndTick < Find.TickManager.TicksGame)
            {
                return ProgressPerTick;
            }

            return ProgressPerTick * hungryGraveConsumeMult;
        }
    }
}
