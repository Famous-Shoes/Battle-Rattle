using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace BattleRattle.BattleFieldMedicine {
  public class Treat_WorkGiver : WorkGiver_Treat {

    public const float SIGNIFICANT_BLEEDING = 0.1f;

    public Treat_WorkGiver(WorkGiverDef giverDef) : base(giverDef) {}


    public override bool HasJobOnThing(Pawn responder, Thing thing) {
      var patient = thing as Pawn;
      if (patient == null) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on null patient. No job."
        );
        #endif

        return false;
      }

      if (!patient.RaceProps.humanoid) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient + ". Not humanoid; no job."
        );
        #endif

        return false;
      }

      if (patient.playerController != null && patient.playerController.Drafted) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient + ". Patient drafted; no job."
        );
        #endif

        return false;
      }

      if (!patient.Downed) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient + ". Patient not downed; no job."
        );
        #endif

        return false;
      }

      if (!PawnNeedsImmediateTreatment(patient)) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient + ". Patient doesn't need treatment; no job."
        );
        #endif

        return false;
      }

      if (!responder.CanReserveAndReach(patient, ReservationType.Heal, PathMode.Touch, Danger.Deadly)) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient 
          + ". Responder cannot reach and reserve patient; no job."
        );
        #endif

        return false;
      }

      var medicine = GetClosestTraumaKit(responder, patient);
      if (medicine == null) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient 
          + ". No trauma kit availble to " + responder + "; no job."
        );
        #endif
      }

      if (!responder.CanReserveAndReach(medicine, ReservationType.Total, PathMode.Touch, Danger.Deadly)) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient 
          + ". Responder cannot reach and reserve trauma kit; no job."
        );
        #endif

        return false;
      }

      return true;
    }

    public override Job JobOnThing(Pawn responder, Thing thing) {
      var patient = thing as Pawn;

      if (GenMedicine.PatientGetsMedicine(patient) && Medicine.GetMedicineCountToFullyHeal(patient) > 0) {
        var medicine = GetClosestTraumaKit(responder, patient);
        if (medicine != null) {
          #if DEBUG
          Log.Message(
            "Creating treatment job by " + responder + " on patient " + patient 
            + " with medicine " + medicine + "."
          );
          #endif

          return new Job(
            DefDatabase<JobDef>.GetNamed("BattleRattle_BattleFieldMedicine_Treat_JobDriver", true),
            patient, 
            medicine
          );
        }
      }

      #if DEBUG
      Log.Message(
        "No trauma kit available to " + responder + " for patient " + patient  
        + ", not creating treatment job."
      );
      #endif

      return null;
    }

    private static Thing GetClosestTraumaKit(Pawn responder, Pawn patient) {
      Predicate<Thing> predicate = (Thing x) => 
        !x.IsForbidden(responder.Faction) 
        && responder.AwareOf(x) 
        && responder.CanReserve(x, ReservationType.Total)
      ;

      return GenClosest.ClosestThing_Global_Reachable(
        patient.Position, 
        Find.ListerThings.ThingsOfDef(ThingDef.Named("BattleRattle_BattleFieldMedicine_TraumaKit")), 
        PathMode.ClosestTouch, 
        TraverseParms.For(responder, Danger.Deadly, true), 
        9999f, 
        predicate
      );
    }

    private static bool PawnNeedsImmediateTreatment(Pawn pawn) {
      if (!pawn.healthTracker.ShouldGetTreatment) {
        #if DEBUG
        Log.Message(
          "Patient " + pawn + " doesn't need treatment according to their "
          + " health tracker."
        );
        #endif

        return false;
      }
      float bleedingRate = pawn.healthTracker.hediffSet.BleedingRate;
      if (bleedingRate < SIGNIFICANT_BLEEDING) {
        #if DEBUG
        Log.Message(
          "Patient " + pawn + " has a bleeding rate of less than 0.1 and so"
          + " doesn't need treatment."
        );
        #endif

        return false;
      }

      return true;
    }
  }
}

