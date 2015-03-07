using System;

using BattleRattle.Apparel;
using System.Collections.Generic;
using Verse;
using BattleRattle.Utility;
using RimWorld;
using System.Collections;

namespace BattleRattle.Pouches {
  public class IFAK:AbstractApparel {

    private Medicine medicine;

    public override void PostMake () {
      base.PostMake ();
      this.medicine = (Medicine) ThingMaker.MakeThing(ThingDefOf.Medicine);
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

        use.action = Use;
        use.icon = Buttons.Icon(this, "Use");
        use.defaultLabel = "Use " + Labels.ForTitleBrief(this);
        use.defaultDesc = "Have " + this.wearer.Nickname + " use the " 
          + Labels.ForSentenceBrief(this) + ".";
        use.activateSound = SoundDef.Named("Click");
      
        yield return use;
      }
    }

    private void Use() {
      Medicine.ApplyOnPawn(this.medicine, this.wearer, this.wearer);
      this.medicine = null;
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
