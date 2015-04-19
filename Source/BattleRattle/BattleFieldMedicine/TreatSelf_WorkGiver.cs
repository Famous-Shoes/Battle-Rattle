using System;
using RimWorld;
using Verse;
using Verse.AI;
using BattleRattle.Pouches;

namespace BattleRattle.BattleFieldMedicine {
  public class TreatSelf_WorkGiver : WorkGiver_Treat {

    public const float SIGNIFICANT_BLEEDING = 0.1f;
    public const float MAX_DISTANCE_TO_TRAVEL_TO_BED = 50f;

    public override ThingRequest PotentialWorkThingRequest {
      get {
        return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
      }
    }

    public override bool HasJobOnThing(Pawn responder, Thing thing) {
      var patient = thing as Pawn;
      if (patient != responder) {
        #if DEBUG
        Log.Message(
          "Checked for self-medication job by " + responder + " on " + patient
          + "; not the same, no job."
        );
        #endif

        return false;
      }

      if (patient.playerController != null && patient.playerController.Drafted) {
        #if DEBUG
        Log.Message(
          "Checked for self-medication job by " + responder + " on " + patient
          + "; not the same, no job."
        );
        #endif

        return false;
      }

      if (!NeedImmediateTreatment(patient)) {
        #if DEBUG
        Log.Message(
          "Checked for self-medication job for " + responder 
          + " but doesn't need treatment; no job."
        );
        #endif

        return false;
      }

      if (IFAK.UsableFrom(patient) == null) {
        #if DEBUG
        Log.Message(
          "Checked for self-medication job for " + responder 
          + " but isn't wearing an IFAK or IFAK is empty; no job."
        );
        #endif

        return false;
      }

      var nearbyMedicalBed = MedicalBestClosestPatient(patient);
      if (nearbyMedicalBed != null) {

        var doctorNearbyMedicalBed = DoctorClosestThing(patient, nearbyMedicalBed);
        if (doctorNearbyMedicalBed != null) {
          #if DEBUG
          Log.Message(
            "Checked for self-medication job for " + responder 
            + " but medical bed (" + nearbyMedicalBed + ") with nearby doctor ("
            + doctorNearbyMedicalBed + ") too close; no job."
          );
          #endif

          return false;
        }
      }
           
      return true;
    }

    public override Job JobOnThing(Pawn patient, Thing thing) {
      if (patient != thing) {
        Log.Warning(
          "Self-treatment job requested for " + patient 
          + " on something else (" + thing + "); not creating job."
        );

        return null;
      }

      var ifak = IFAK.UsableFrom(patient);
      if (ifak != null) {
        #if DEBUG
        Log.Message(
          "Creating self-treatment job by " + patient + " with medicine "
          + ifak + "."
        );
        #endif

        var job = new Job(
          TreatWithIFAK_JobDriver.Def,
          patient, 
          ifak
        );

        job.maxNumToCarry = 1;
        return job;
      }

      #if DEBUG
      Log.Message(
        "No trauma kit available to " + patient 
        + " for self-treatment, not creating job."
      );
      #endif

      return null;
    }

    private static bool OnlyFriendlyDoctors(Pawn patient, Thing thing) {
      var doctor = thing as Pawn;
      if (doctor == null) {
        return false;
      }

      return doctor.Faction == patient.Faction
        && doctor.workSettings.WorkIsActive(WorkTypeDefOf.Doctor)
        && !doctor.Downed;
    }

    private static Thing DoctorClosestThing(Pawn patient, Thing thing) {
      return GenClosest.ClosestThing_Global_Reachable(
        thing.Position, 
        Find.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn),
        PathMode.ClosestTouch, 
        TraverseParms.For(patient, Danger.Deadly, true), 
        MAX_DISTANCE_TO_TRAVEL_TO_BED, 
        (x) => OnlyFriendlyDoctors(patient, x)
      );
    }

    private static Thing MedicalBestClosestPatient(Pawn patient) {
      Predicate<Thing> onlyMedicalBeds = (Thing x) => 
        x is Building_Bed
        && ((Building_Bed) x).Medical
        && !x.IsForbidden(patient.Faction) 
        && patient.AwareOf(x) 
        && patient.CanReserve(x)
      ;

      return GenClosest.ClosestThing_Global_Reachable(
        patient.Position, 
        Find.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial), 
        PathMode.ClosestTouch, 
        TraverseParms.For(patient, Danger.Deadly, true), 
        MAX_DISTANCE_TO_TRAVEL_TO_BED, 
        onlyMedicalBeds
      );
    }

    private static bool NeedImmediateTreatment(Pawn pawn) {
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
