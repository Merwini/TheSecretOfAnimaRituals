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
    protected override float HeatPerSecond => base.HeatPerSecond * GameComponent_AnimaRitual.Instance.flowerPowerMult;

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
        int ticksLeft = GameComponent_AnimaRitual.Instance.flowerPowerEndTick - Find.TickManager.TicksGame;
        if (ticksLeft < 0)
        {
            return "TSOA_FlowerPowerInactive".Translate();
        }

        return base.CompInspectStringExtra() + "\n" + "TSOA_FlowerPowerDuration".Translate(ticksLeft.ToStringTicksToDays());
    }
}
