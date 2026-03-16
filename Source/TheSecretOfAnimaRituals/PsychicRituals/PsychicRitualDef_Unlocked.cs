using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace tsoa.rituals;

public class PsychicRitualDef_Unlocked : PsychicRitualDef_InvocationCircle
{
    public PsychicRitual currentPsychicRitual; // it doesn't seem like PsychicRitualDef stores this anywhere, it only passes through this class via CreateToils

    public List<ThingDef> ritualFocuses; // List of allowed ritual focuses for this ritual
    public Thing ritualFocus; // The actual focus used in the ritual instance

    public ResearchProjectDef advancedResearchProject;

    public bool targetsCell;
    public IntVec3 targetCell;

    public bool targetsPawn;
    public Pawn targetPawn;

    public ThingDef targetsThingOfDef;
    public Thing targetThing;

    public int invokerAffinity = 5;
    public int targetAffinity = 3;
    public int chanterAffinity = 1;

    public bool gizmoMakesFloatMenu;
    public virtual List<string> FloatMenuOptionStrings => null;
    public string selectedFloatMenuOptionString = null;

    public virtual bool ExtraCellValidator(IntVec3 cell, Map map)
    {
        return true;
    }

    public virtual bool ExtraThingValidator(Thing thing)
    {
        return true;
    }

    public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
    {
        currentPsychicRitual = psychicRitual;

        return base.CreateToils(psychicRitual, graph);
    }

    public override TaggedString PsychicRitualCompletedMessage()
    {
        // TODO update affinity for participants
        if (currentPsychicRitual != null && Current.Game.GetComponent<GameComponent_AffinityTracker>() is GameComponent_AffinityTracker gameComp)
        {
            IEnumerable<Pawn> invokers = currentPsychicRitual.assignments.AssignedPawns(InvokerRole);
            foreach (Pawn invoker in invokers)
            {
                gameComp.UpdateAnimaAffinityFor(invoker, invokerAffinity);
            }

            IEnumerable<Pawn> targets = currentPsychicRitual.assignments.AssignedPawns(TargetRole);
            foreach (Pawn target in targets)
            {
                gameComp.UpdateAnimaAffinityFor(target, targetAffinity);
            }

            IEnumerable<Pawn> chanters = currentPsychicRitual.assignments.AssignedPawns(ChanterRole);
            foreach (Pawn chanter in chanters)
            {
                gameComp.UpdateAnimaAffinityFor(chanter, chanterAffinity);
            }
        }

        return base.PsychicRitualCompletedMessage();
    }


    // Didn't find a use case, might re-enable later but would require rewriting eruption and beckoning rituals
    //public virtual void HandleFloatMenuOption(string option)
    //{
    //}

    public override void ResolveReferences()
    {
        if (ritualFocuses.NullOrEmpty())
        {
            ritualFocuses = new List<ThingDef>();
            Log.Error($"Config error in {defName} from {modContentPack.Name}. Using class PsychicRitualDef_LocationUnlocked but has no list of ritual focuses.");
        }

        if ((targetsCell && targetsPawn) || (targetsCell && targetsThingOfDef != null) || (targetsPawn && targetsThingOfDef != null))
        {
            Log.Error($"Config error in {defName} from {modContentPack.Name}. Using class PsychicRitualDef_LocationUnlocked but has multiple target types set.");
        }

        if (gizmoMakesFloatMenu && (targetsCell || targetsPawn || targetsThingOfDef != null))
        {
            Log.Error($"Config error in {defName} from {modContentPack.Name}. Using class PsychicRitualDef_LocationUnlocked and is configured to make a float menu AND target something.");
        }

        base.ResolveReferences();
    }
}
