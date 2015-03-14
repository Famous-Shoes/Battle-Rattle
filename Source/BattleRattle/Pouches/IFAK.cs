using System.Collections.Generic;

using RimWorld;
using Verse;

using BattleRattle.Apparel;
using BattleRattle.BattleFieldMedicine;
using BattleRattle.Utility;


namespace BattleRattle.Pouches {
  public class IFAK: AbstractApparel {

    private Medicine medicine;

    public static int CAPACITY = 1;

    public IFAKDef Def {
      get {
        return (IFAKDef) this.def;
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

    public int PackRadius {
      get {
        return Def.packRadius;
      }
    }

    private void Fill() {
      this.medicine = (Medicine) ThingMaker.MakeThing(TraumaKitDef.Instance);
    }

    public void Pack(Thing packing, Pawn packer) {
      if (packing == null || packing.def != TraumaKitDef.Instance) {
        Log.Warning("Tried to pack [" + packing + "] into " + this + ".");
        return;
      }

      if (!this.IsEmpty) {
        Log.Warning("Tried to pack non-empty " + this + ".");
        return;
      }

      if (packing.stackCount <= 0) {
        Log.Error("Tried to pack zero stack of " + packing + " into " + this + ".");
        return;
      }

      var packed = (Medicine) packing;
      if (packing.stackCount > CAPACITY) {
        packed = (Medicine) packing.SplitOff(CAPACITY);
      }

      // FIXME this is too extreme, need to only destroy the trauma kit that
      // was packed.
      packer.carryHands.GetContainer().DestroyContents();
      Fill();
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
