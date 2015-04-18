using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;


namespace BattleRattle.BattleFieldMedicine {
  public class TreatWithMedicine_JobDriver: JobDriver {

    public static JobDef Def {
      get {
        return DefDatabase<JobDef>.GetNamed(
          "BattleRattle_BattleFieldMedicine_TreatWithMedicine_JobDriver", true
        );
      }
    }

    protected override IEnumerable<Toil> MakeNewToils() {
      var patient = this.pawn.CurJob.GetTarget(TargetIndex.A).Thing as Pawn;
      var medicineTarget = this.pawn.CurJob.GetTarget(TargetIndex.B);
      var medicine = medicineTarget.Thing;

      #if DEBUG
      Log.Message("Making toils for " + pawn + " to treat " + patient + " with " + medicine + ".");
      #endif

      this.FailOnDestroyedOrForbidden(TargetIndex.A);
      this.FailOnDestroyedOrForbidden(TargetIndex.B);

      yield return Toils_Reserve.Reserve(TargetIndex.A);
      yield return Toils_Reserve.Reserve(TargetIndex.B);

      #if DEBUG
      Log.Message(" - " + pawn + " will reserve " + patient + " and " + medicine + ".");
      Log.Message(pawn + " moving to " + medicine + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.B, PathMode.Touch);

      #if DEBUG
      Log.Message(" - " + pawn + " will carry " + medicine + ".");
      #endif
      yield return Toils_Heal.PickupMedicine(TargetIndex.B, patient);

      #if DEBUG
      Log.Message(" - " + pawn + " will move to  " + patient + " with " + medicine + ".");
      #endif

      yield return Toils_Goto.GotoThing(TargetIndex.A, PathMode.Touch);

      #if DEBUG
      Log.Message(" - " + pawn + " will treat " + patient + " with " + medicine + ".");
      #endif

      var timeToTreat = TraumaKitDef.Instance.ticksForTreatment;

      for (int i = 0; i < medicine.stackCount; i++) {
        yield return Toils_General.Wait(timeToTreat);
        yield return Toils_Heal.ApplyMedicine(patient);
      }

      yield return Toils_Reserve.Release(TargetIndex.A);
      // This is going to error, but the alternative is to copy a bunch of logic
      // from RimWorld. Really, unreserve should just quietly succeed if the 
      // target doesn't need to be unreserved for some reason rather than getting
      // prissy about it.
      yield return Toils_Reserve.Release(TargetIndex.B);

      #if DEBUG
      Log.Message(" - " + pawn + " will unreserve " + patient + " and " + medicine + ".");
      #endif

      yield break;
    }
  }
}
