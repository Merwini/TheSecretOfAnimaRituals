using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace tsoa.rituals;

public class Hediff_AnimaAffinity : Hediff
{
    private float currentAffinity = 0;

    private HediffStage curStage;
    public override HediffStage CurStage
    {
        get
        {
            if (curStage == null)
            {
                curStage = new HediffStage();
                curStage.statOffsets = new List<StatModifier>()
                {
                    new StatModifier()
                    {
                        stat = StatDefOf.PsychicSensitivity,
                        value = currentAffinity
                    }
                };
            }
            return curStage;
        }
    }

    // TODO these could just be a getter and setter
    public void AddAffinity(float amount)
    {
        currentAffinity += amount;
        curStage = null;
    }

    public float CheckAffinity()
    {
        return currentAffinity;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref currentAffinity, "currentAffinity", 0);

        base.ExposeData();
    }
}
