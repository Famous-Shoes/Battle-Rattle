using System.Collections.Generic;

using Verse;
using Verse.AI;


namespace BattleRattle.Pouches {
  public class PackIFAK_JobDriver: JobDriver {

    public static JobDef Def {
      get {
        return DefDatabase<JobDef>.GetNamed(
          "BattleRattle_Pouches_PackIFAK_JobDriver", true
        );
      }
    }

    public PackIFAK_JobDriver(Pawn pawn): base(pawn) {}

    protected override IEnumerable<Toil> MakeNewToils() {
      var ifak = this.pawn.CurJob.GetTarget(TargetIndex.A).Thing as IFAK;
      var traumaKit = this.pawn.CurJob.GetTarget(TargetIndex.B).Thing;

      #if DEBUG
      Log.Message("Making toils for " + pawn + " to pack " + traumaKit + " into " + ifak + ".");
      #endif

      this.FailOnDestroyedOrForbidden(TargetIndex.A);
      this.FailOnBurningImmobile(TargetIndex.A);
      this.FailOnDestroyedOrForbidden(TargetIndex.B);
      this.FailOnBurningImmobile(TargetIndex.B);

      yield return Toils_Reserve.Reserve(TargetIndex.A, ReservationType.Use);
      yield return Toils_Reserve.Reserve(TargetIndex.B, ReservationType.Total);

      #if DEBUG
      Log.Message(" - " + pawn + " will reserve " + traumaKit + " and " + ifak + ".");
      Log.Message(" - " + pawn + " will move to " + traumaKit + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.B, PathMode.Touch);

      #if DEBUG
      Log.Message(" - " + pawn + " carry " + traumaKit + ".");
      #endif
      yield return Toils_Haul.StartCarryThing(TargetIndex.B);

      #if DEBUG
      Log.Message(" - " + pawn + " will move to  " + ifak + " with " + traumaKit + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.A, PathMode.Touch);
      yield return Toils_General.Wait(IFAKDef.Instance.ticksForPacking);

      #if DEBUG
      Log.Message(" - " + pawn + " will pack " + traumaKit + " into " + ifak + ".");
      #endif
      var packToil = new Toil();
      packToil.initAction = delegate {
        ifak.Pack(traumaKit, this.pawn);
      };
      packToil.defaultCompleteMode = ToilCompleteMode.Instant;
      yield return packToil;

      yield return Toils_Reserve.Unreserve(TargetIndex.A, ReservationType.Use);
      yield return Toils_Reserve.Unreserve(TargetIndex.B, ReservationType.Total);

      #if DEBUG
      Log.Message(" - " + pawn + " will unreserve " + traumaKit + " and " + ifak + ".");
      #endif

      yield break;
    }
  }
}
