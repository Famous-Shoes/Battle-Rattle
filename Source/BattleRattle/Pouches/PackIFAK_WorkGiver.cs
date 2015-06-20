using System;

using RimWorld;
using Verse;
using Verse.AI;
using BattleRattle.BattleFieldMedicine;
using BattleRattle.Pouches;

namespace BattleRattle.Pouches {
  public class PackIFAK_WorkGiver: WorkGiver {

    public override ThingRequest PotentialWorkThingRequest {
      get {
        return ThingRequest.ForDef(IFAKDef.Instance);
      }
    }

    public override Job JobOnThing (Pawn pawn, Thing thing) {
      var ifak = thing as IFAK;
      if (ifak == null) {
        #if DEBUG
        Log.Message(
          "Checked for pack IFAK job on " + thing + ": no job, not an IFAK."
        );
        #endif

        return null;
      }

      if (!ifak.IsEmpty) {
        #if DEBUG
        Log.Message(
          "Checked for pack IFAK job on " + thing + ": no job, IFAK is not empty."
        );
        #endif

        return null;
      }

      if (!pawn.CanReserveAndReach(thing, PathEndMode.Touch, pawn.NormalMaxDanger())) {
        #if DEBUG
        Log.Message(
          "Checked for pack IFAK job on " + ifak 
          + ": no job, pawn cannot reach and reserve it."
        );
        #endif

        return null;
      }

      var closest = GenClosest.ClosestThingReachable(
        ifak.Position, 
        ThingRequest.ForDef(TraumaKitDef.Instance), 
        PathEndMode.Touch, 
        TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 
        ifak.PackRadius
      );

      if (closest == null) {
        #if DEBUG
        Log.Message(
          "Checked for pack IFAK job on " + ifak 
          + ": no job, no trauma kit found nearby."
        );
        #endif

        return null;
      }

      if (!pawn.CanReserveAndReach(closest, PathEndMode.Touch, pawn.NormalMaxDanger())) {
        #if DEBUG
        Log.Message(
          "Checked for pack IFAK job on " + ifak 
          + ": no job, pawn cannot reach and reserve a trauma kit."
        );
        #endif

        return null;
      }

      var job = new Job(PackIFAK_JobDriver.Def, thing, closest);
      job.maxNumToCarry = IFAK.CAPACITY;

      return job;
    }

  }
}
