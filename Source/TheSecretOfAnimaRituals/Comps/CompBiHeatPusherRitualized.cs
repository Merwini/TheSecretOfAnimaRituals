using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsoa.core;
using Verse;
using RimWorld;

namespace tsoa.rituals;

public class CompBiHeatPusherRitualized : CompBiHeatPusher
{
    public static GameComponent_PsychicRitualManager psychicRitualManager;

    public override bool ShouldPushHeatNow(out float temperature)
    {
        if (GameComponent_AnimaRitual.Instance.flowerPowerEndTick < Find.TickManager.TicksGame)
        {
            temperature = 0;
            return false;
        }

        return base.ShouldPushHeatNow(out temperature);
    }

    public override string CompInspectStringExtra()
    {
        if (GameComponent_AnimaRitual.Instance.flowerPowerEndTick < Find.TickManager.TicksGame)
        {
            return "TSOA_FlowerPowerInactive".Translate();
        }

        return base.CompInspectStringExtra();
    }
}
