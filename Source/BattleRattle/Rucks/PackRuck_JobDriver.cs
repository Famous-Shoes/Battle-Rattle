using System.Collections.Generic;

using Verse;
using Verse.AI;


namespace BattleRattle.Rucks {
  public class PackRuck_JobDriver: JobDriver {

    public PackRuck_JobDriver(Pawn pawn): base(pawn) {}

    protected override IEnumerable<Toil> MakeNewToils() {
      var ruck = this.pawn.CurJob.GetTarget(TargetIndex.A).Thing as IRuck;
      var packing = this.pawn.CurJob.GetTarget(TargetIndex.B).Thing;

      #if DEBUG
      Log.Message("Making toils for " + pawn + " to pack " + packing + " into " + ruck + ".");
      #endif

      this.FailOnDestroyedOrForbidden(TargetIndex.A);
      this.FailOnBurningImmobile(TargetIndex.A);
      this.FailOnDestroyedOrForbidden(TargetIndex.B);
      this.FailOnBurningImmobile(TargetIndex.B);
//      this.FailOn(ruck.NotPackable);
//      this.FailOn(() => ruck.CheckNotPackable(packing));

      yield return Toils_Reserve.Reserve(TargetIndex.A, ReservationType.Use);
      yield return Toils_Reserve.Reserve(TargetIndex.B, ReservationType.Total);

      #if DEBUG
      Log.Message(pawn + " reserved " + packing + " and " + ruck + ".");
      Log.Message(pawn + " moving to " + packing + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.B, PathMode.Touch);

      #if DEBUG
      Log.Message(pawn + " carrying " + packing + ".");
      #endif
      yield return Toils_Haul.StartCarryThing(TargetIndex.B);

      #if DEBUG
      Log.Message(pawn + " moving to  " + ruck + " with " + packing + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.A, PathMode.Touch);

      #if DEBUG
      Log.Message(pawn + " packing " + packing + " into " + ruck + ".");
      #endif
      var packToil = new Toil();
      packToil.initAction = delegate {
        ruck.Pack(pawn, packing);
      };
      packToil.defaultCompleteMode = ToilCompleteMode.Instant;
      yield return packToil;

      yield return Toils_Reserve.Unreserve(TargetIndex.A, ReservationType.Use);
      yield return Toils_Reserve.Unreserve(TargetIndex.B, ReservationType.Total);

      #if DEBUG
      Log.Message(pawn + " unreserved " + packing + " and " + ruck + ".");
      #endif

      yield break;
    }
  }
}
