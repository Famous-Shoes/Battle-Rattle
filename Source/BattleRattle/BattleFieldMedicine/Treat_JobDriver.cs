using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;


namespace BattleRattle.BattleFieldMedicine {
  public class Treat_JobDriver: JobDriver {

    public Treat_JobDriver(Pawn pawn): base(pawn) {}

    protected override IEnumerable<Toil> MakeNewToils() {
      var patient = this.pawn.CurJob.GetTarget(TargetIndex.A).Thing as Pawn;
      var medicine = this.pawn.CurJob.GetTarget(TargetIndex.B).Thing;

      #if DEBUG
      Log.Message("Making toils for " + pawn + " to treat " + patient + " with " + medicine + ".");
      #endif

      this.FailOnDestroyedOrForbidden(TargetIndex.A);
      this.FailOnDestroyedOrForbidden(TargetIndex.B);

      yield return Toils_Reserve.Reserve(TargetIndex.A, ReservationType.Use);
      yield return Toils_Reserve.Reserve(TargetIndex.B, ReservationType.Total);

      #if DEBUG
      Log.Message(pawn + " reserved " + patient + " and " + medicine + ".");
      Log.Message(pawn + " moving to " + medicine + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.B, PathMode.Touch);

      #if DEBUG
      Log.Message(pawn + " carrying " + medicine + ".");
      #endif
      yield return Toils_Heal.PickupMedicine(TargetIndex.B, patient);

      #if DEBUG
      Log.Message(pawn + " moving to  " + patient + " with " + medicine + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.A, PathMode.Touch);

      #if DEBUG
      Log.Message(pawn + " treating " + patient + " with " + medicine + ".");
      #endif

      yield return Toils_Heal.ApplyMedicine(patient);

      yield return Toils_Reserve.Unreserve(TargetIndex.A, ReservationType.Use);
      // This is going to error, but the alternative is to copy a bunch of logic
      // from RimWorld. Really, unreserve should just quietly succeed if the 
      // target doesn't need to be unreserved for some reason rather than getting
      // prissy about it.
      yield return Toils_Reserve.Unreserve(TargetIndex.B, ReservationType.Total);

      #if DEBUG
      Log.Message(pawn + " unreserved " + patient + " and " + medicine + ".");
      #endif

      yield break;
    }
  }
}
