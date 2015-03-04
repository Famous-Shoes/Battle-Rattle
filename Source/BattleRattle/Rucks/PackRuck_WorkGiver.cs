using System;

using RimWorld;
using Verse;
using Verse.AI;

namespace BattleRattle.Rucks {
  public class PackRuck_WorkGiver: WorkGiver {

    public PackRuck_WorkGiver(WorkGiverDef def): base(def) {}

    public override ThingRequest PotentialWorkThingRequest {
      get {
        return ThingRequest.ForDef(ThingDef.Named("BattleRattle_Rucks_LongRange"));
      }
    }

    public override Job JobOnThing (Pawn pawn, Thing thing) {
      var ruck = thing as IRuck;
      if (ruck == null) {
        #if DEBUG
        Log.Message("Checked for pack ruck job on " + thing + ": no job, not a ruck.");
        #endif

        return null;
      }

      if (!ruck.IsPackable) {
        #if DEBUG
        Log.Message("Checked for pack ruck job on " + thing + ": no job, ruck is not packable.");
        #endif

        return null;
      }

      if (!pawn.CanReserveAndReach(thing, ReservationType.Use, PathMode.Touch, pawn.NormalMaxDanger())) {
        #if DEBUG
        Log.Message("Checked for pack ruck job on " + ruck + ": no job, pawn cannot reach and reserve it.");
        #endif

        return null;
      }

      var closest = GenClosest.ClosestThingReachable(
        ruck.Position, 
        ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), 
        PathMode.Touch, 
        TraverseParms.For(pawn, pawn.NormalMaxDanger(), false), 
        ruck.PackRadius, 
        new Predicate<Thing>(ruck.CheckPackable)
      );


      if (closest == null) {
        #if DEBUG
        Log.Message("Nothing found from closest search, no job.");
        #endif
        return null;
      }

      var job = new Job(DefDatabase<JobDef>.GetNamed("BattleRattle_Rucks_PackRuck", true), thing, closest);
      job.maxNumToCarry = ruck.CanFit(closest);

      #if DEBUG
      Log.Message("Can carry " + job.maxNumToCarry + " of " + closest + " to " + ruck + ".");
      #endif

      var alternateDropCell = IntVec3.Invalid;
      StoreUtility.TryFindBestBetterStoreCellFor(
        thing, pawn, StoragePriority.Unstored, pawn.Faction, out alternateDropCell
      );
      
      if (alternateDropCell != IntVec3.Invalid) {
        job.targetC = alternateDropCell;
      }

      return job;
    }

  }
}
  