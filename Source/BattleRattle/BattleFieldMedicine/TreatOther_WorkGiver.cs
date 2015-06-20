using System;
using RimWorld;
using Verse;
using Verse.AI;
using BattleRattle.Pouches;

namespace BattleRattle.BattleFieldMedicine {
  public class TreatOther_WorkGiver : WorkGiver_Treat {

    public const float SIGNIFICANT_BLEEDING = 0.1f;

    public TreatOther_WorkGiver() : base() {}

    public override ThingRequest PotentialWorkThingRequest {
      get {
        return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
      }
    }

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

      if (patient == responder) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on self. No job."
        );
        #endif

        return false;
      }

      if (!patient.RaceProps.Humanlike) {
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

      if (!responder.CanReserveAndReach(patient, PathEndMode.Touch, Danger.Deadly)) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient 
          + ". Responder cannot reach and reserve patient; no job."
        );
        #endif

        return false;
      }

      Thing medicine = GetMedicine(responder, patient);
      if (medicine == null) {
        #if DEBUG
        Log.Message(
          "Checked for battlefield medicine job by " + responder 
          + " on patient " + patient 
          + ". No trauma kit or IFAK availble to " + responder + "; no job."
        );
        #endif

        return false;
      }

      if (!responder.CanReserveAndReach(medicine, PathEndMode.Touch, Danger.Deadly)) {
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
        var medicine = GetMedicine(responder, patient);
        if (medicine == null) {
          #if DEBUG
          Log.Message(
            "No trauma kit available to " + responder + " for patient " + patient  
            + ", not creating treatment job."
          );
          #endif

          return null;
        }

        #if DEBUG
        Log.Message(
          "Creating treatment job by " + responder + " on patient " + patient 
          + " with medicine " + medicine + "."
        );
        #endif

        Job job;

        if (medicine is IFAK) {
          job = new Job(TreatWithIFAK_JobDriver.Def, patient, medicine);
          job.maxNumToCarry = 1;
        
        } else {
          job = new Job(TreatWithMedicine_JobDriver.Def, patient, medicine);
          job.maxNumToCarry = Medicine.GetMedicineCountToFullyHeal(patient);
        }

        return job;
      }

      return null;
    }

    private static Thing GetMedicine(Pawn responder, Pawn patient) {
      Thing medicine = GetClosestTraumaKit(responder, patient);
      if (medicine == null) {
        medicine = GetClosestIFAK(responder, patient);
      }

      return medicine;
    }

    private static Thing GetClosestTraumaKit(Pawn responder, Pawn patient) {
      Predicate<Thing> onlyUsableTraumaKits = (Thing x) => 
        !x.IsForbidden(responder.Faction) 
        && responder.AwareOf(x) 
        && responder.CanReserve(x)
      ;

      return GenClosest.ClosestThing_Global_Reachable(
        patient.Position, 
        Find.ListerThings.ThingsOfDef(TraumaKitDef.Instance), 
        PathEndMode.ClosestTouch, 
        TraverseParms.For(responder, Danger.Deadly, TraverseMode.ByPawn, true), 
        9999f, 
        onlyUsableTraumaKits
      );
    }

    private static Thing GetClosestIFAK(Pawn responder, Pawn patient) {
      Predicate<Thing> onlyUsableIFAKs = (Thing x) => 
        !x.IsForbidden(responder.Faction) 
        && !((IFAK) x).IsEmpty
        && responder.AwareOf(x) 
        && responder.CanReserve(x)
      ;

      return FindWornIFAK(responder, patient) ?? GenClosest.ClosestThing_Global_Reachable(
        patient.Position, 
        Find.ListerThings.ThingsOfDef(IFAKDef.Instance), 
        PathEndMode.ClosestTouch, 
        TraverseParms.For(responder, Danger.Deadly, TraverseMode.ByPawn, true), 
        9999f, 
        onlyUsableIFAKs
      );
    }

    private static Thing FindWornIFAK(Pawn responder, Pawn patient) {
      return IFAK.UsableFrom(patient) ?? IFAK.UsableFrom(responder);
    }

    private static bool PawnNeedsImmediateTreatment(Pawn pawn) {
      if (!pawn.health.ShouldGetTreatment) {
        #if DEBUG
        Log.Message(
          "Patient " + pawn + " doesn't need treatment according to their "
          + " health tracker."
        );
        #endif

        return false;
      }
      float bleedingRate = pawn.health.hediffSet.BleedingRate;
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

