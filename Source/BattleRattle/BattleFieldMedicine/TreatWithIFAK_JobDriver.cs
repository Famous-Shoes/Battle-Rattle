using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;
using BattleRattle.Pouches;


namespace BattleRattle.BattleFieldMedicine {
  public class TreatWithIFAK_JobDriver: JobDriver {

    public TreatWithIFAK_JobDriver(Pawn pawn): base() {}

    public static JobDef Def {
      get {
        return DefDatabase<JobDef>.GetNamed(
          "BattleRattle_BattleFieldMedicine_TreatWithIFAK_JobDriver", true
        );
      }
    }

    protected override IEnumerable<Toil> MakeNewToils() {
      var responder = this.pawn;
      var patient = responder.CurJob.GetTarget(TargetIndex.A).Thing as Pawn;
      var ifakTarget = responder.CurJob.GetTarget(TargetIndex.B);
      var ifak = ifakTarget.Thing as IFAK;

      if (ifak == null) {
        Log.Error(
          "Treat with IFAK job started by (" + responder
          + ") with an object not an IFAK (" + ifakTarget.Thing + ".)"
        );

        yield break;
      }

      #if DEBUG
      Log.Message(
        "Making toils for " + responder + " to treat " + patient + " with " + ifak + "."
      );
      #endif

      this.FailOnDestroyedOrForbidden(TargetIndex.A);
      this.FailOnDestroyedOrForbidden(TargetIndex.B);

      yield return Toils_Reserve.Reserve(TargetIndex.A);
      yield return Toils_Reserve.Reserve(TargetIndex.B);

      #if DEBUG
      Log.Message(" - " + responder + " will reserve " + patient + " and " + ifak + ".");
      #endif

      var ifakWorn = (ifak.wearer == responder || ifak.wearer == patient);
      if (!ifakWorn) {
        #if DEBUG
        Log.Message(" - " + responder + " will move to " + ifak + ".");
        #endif

        yield return Toils_Goto.GotoThing(TargetIndex.B, PathMode.Touch);

        #if DEBUG
        Log.Message(" - " + responder + " will pick up " + ifak + ".");
        #endif
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
      }

      if (responder != patient) {
        #if DEBUG
        Log.Message(" - " + responder + " will move to  " + patient + " with " + ifak + ".");
        #endif

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathMode.Touch);
      }

      yield return Toils_Reserve.Release(TargetIndex.B);

      #if DEBUG
      Log.Message(" - " + responder + " will treat " + patient + " with " + ifak.PackedKit + ".");
      #endif

      yield return Toils_General.Wait(TraumaKitDef.Instance.ticksForTreatment);

      var applyIFAK = new Toil();
      applyIFAK.initAction = delegate {
        ifak.UseOn(patient, responder);
      };
      applyIFAK.defaultCompleteMode = ToilCompleteMode.Instant;
      yield return applyIFAK;

      yield return Toils_Reserve.Release(TargetIndex.A);
      // This is going to error, but the alternative is to copy a bunch of logic
      // from RimWorld. Really, unreserve should just quietly succeed if the 
      // target doesn't need to be unreserved for some reason rather than getting
      // prissy about it.
      yield return Toils_Reserve.Release(TargetIndex.B);

      #if DEBUG
      Log.Message(" - " + responder + " will unreserve " + patient + " and " + ifak + ".");
      #endif

      yield break;
    }
  }
}
