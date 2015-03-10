using System;

using BattleRattle.Apparel;
using System.Collections.Generic;
using Verse;
using BattleRattle.Utility;
using RimWorld;
using System.Collections;
using BattleRattle.BattleFieldMedicine;

namespace BattleRattle.Pouches {
  public class IFAK: AbstractApparel {

    private Medicine medicine;

    public static ThingDef Def {
      get {
        return ThingDef.Named("BattleRattle_Pouches_IFAK");
      }
    }

    public static IFAK UsableFrom(Pawn target) {
      foreach (RimWorld.Apparel apparel in target.apparel.WornApparel) {
        var ifak = apparel as IFAK;
        if (ifak != null && !ifak.IsEmpty) {
          return ifak;
        }
      }

      return null;
    }

    public override void PostMake () {
      base.PostMake ();
      this.medicine = (Medicine) ThingMaker.MakeThing(TraumaKitDef.Instance);
    }

    public override IEnumerable<Verse.Gizmo> GetWornGizmos () {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      if (this.wearer.Downed) {
        yield break;
      }

      if (!this.IsEmpty && this.wearer.healthTracker.ShouldGetTreatment) {
        var use = new Command_Action();

        use.action = () => UseOn(this.wearer, this.wearer);
        use.icon = Buttons.Icon(this, "Use");
        use.defaultLabel = "Use " + Labels.ForTitleBrief(this);
        use.defaultDesc = "Have " + this.wearer.Nickname + " use the " 
          + Labels.ForSentenceBrief(this) + ".";
        use.activateSound = SoundDef.Named("Click");
      
        yield return use;
      }
    }

    public void UseOn(Pawn patient, Pawn responder) {
      if (this.medicine != null) {
        Medicine.ApplyOnPawn(this.medicine, patient, responder);
        this.medicine = null;
      }
    }

    public Medicine PackedKit {
      get {
        return this.medicine;
      }
    }

    public bool IsEmpty {
      get {
        return this.medicine == null;
      }
    }

    public override string Label {
      get {
        if (IsEmpty) {
          return base.Label + " (Empty)";
        }

        return base.Label;
      }
    }

  }
}
