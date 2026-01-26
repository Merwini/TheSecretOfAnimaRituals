using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace tsoa.rituals;

public class HediffComp_PsychicFever : HediffComp
{
    private int ticksRemaining = -1;

    public HediffCompProperties_PsychicFever Props => (HediffCompProperties_PsychicFever)props;

    public override void CompPostMake()
    {
        base.CompPostMake();
        InitializeDurationIfNeeded();
    }

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        InitializeDurationIfNeeded();
    }

    private void InitializeDurationIfNeeded()
    {
        if (ticksRemaining >= 0)
            return;

        float days = Props.durationDays > 0f ? Props.durationDays : (Props.durationDaysRandom.TrueMin > 0f ? Props.durationDaysRandom.RandomInRange : 2f);

        ticksRemaining = Mathf.Max(1, Mathf.RoundToInt(days * GenDate.TicksPerDay));
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        if (ticksRemaining > 0)
        {
            ticksRemaining--;
            if (ticksRemaining == 0)
            {
                Pawn.health.RemoveHediff(parent);
                return;
            }
        }

        float ticksToLethal = Mathf.Max(1f, Props.hoursToLethal * GenDate.TicksPerHour);
        float perTick = 1f / ticksToLethal;

        severityAdjustment += perTick;
    }

    public override void CompTended(float quality, float maxQuality, int batchPosition = 0)
    {
        base.CompTended(quality, maxQuality, batchPosition);

        parent.Severity = Props.severityAfterTend;
    }

    public override string CompTipStringExtra
    {
        get
        {
            if (ticksRemaining < 0)
                return null;

            return "TSOA_PsychicFever_TimeRemaining".Translate(ticksRemaining.ToStringTicksToPeriod(allowSeconds: false, shortForm: true));
        }
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);
    }

}
