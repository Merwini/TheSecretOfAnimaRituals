using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace tsoa.rituals;

public class GameComponent_AnimaRitual : GameComponent
{
    public static GameComponent_AnimaRitual Instance;

    public int flowerPowerEndTick = -1;
    public float flowerPowerMult = 1f;

    public GameComponent_AnimaRitual(Game game)
    {
    }

    public override void FinalizeInit()
    {
        Instance = this;

        base.FinalizeInit();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref flowerPowerEndTick, "flowerPowerEndTick", -1);
        Scribe_Values.Look(ref flowerPowerMult, "flowerPowerMult", 1f);

        base.ExposeData();
    }
}
