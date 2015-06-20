using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using RimWorld;
using Verse;

using BattleRattle.Apparel;
using BattleRattle.Utility;


namespace BattleRattle.WeaponCarriers {
  public class WeaponCarrier: AbstractApparel {
  
    private Regex storableRegex;

    private Command_Action storePrimaryGizmo;

    private ThingContainer container;
    private WeaponCarrierDef carrierDef;


    public WeaponCarrierDef Def {
      get {
        if (this.carrierDef == null) {
          this.carrierDef = (WeaponCarrierDef) this.def;
        }

        return this.carrierDef;
      }
    }


    #region Lifecycle

    public override void PostMake() {
      base.PostMake();

      this.container = new ThingContainer();
    }

    public override void ExposeData() {
      base.ExposeData();

      Scribe_Deep.LookDeep(ref this.container, "container");
    }

    #endregion


    #region Storing

    public virtual bool CanStoreThing(Thing thing) {
      return thing != null && StorableRegex.IsMatch(thing.def.defName);
    }

    public Regex StorableRegex {
      get {
        if (this.storableRegex == null) {
          this.storableRegex = new Regex(
            Def.storablePattern, RegexOptions.IgnoreCase
          );
        }

        return this.storableRegex;
      }
    }

    #endregion


    #region Actions

    public virtual bool StorePrimary() {
      ThingWithComps transferred;

      var success = this.wearer.equipment.TryTransferEquipmentToContainer(
        this.wearer.equipment.Primary, this.container, out transferred
      );

      if (!success) {
        Log.Warning(
          "Unable to store primary (" + this.wearer.equipment.Primary 
          + ") in carrier's container (" + this.container + ")."
        );
      }

      return true;
    }

    public virtual bool EquipPrimary(Thing stored) {
      ThingWithComps dropped;
      if (this.wearer.equipment.Primary != null) {
        var success = this.wearer.equipment.TryDropEquipment(
          this.wearer.equipment.Primary, out dropped, this.wearer.Position
        );

        if (!success) {
          Messages.Message(
            this.wearer.Nickname + " cannot equip "
            + Labels.ForSentenceBrief(stored) + " because they have " 
            + Labels.ForSentenceBrief(this.wearer.equipment.Primary)
            + " equipped and no place to drop that.", MessageSound.Silent
          );
          return false;
        }
      }

      this.container.Remove(stored);
      this.wearer.equipment.AddEquipment((ThingWithComps) stored);

      return true;
    }

    public bool RemoveStored(Thing stored) {
      if (!this.SpawnedInWorld) {
        return false;
      }

      Thing dropped;
      var success = this.container.TryDrop(
        stored, this.Position, ThingPlaceMode.Near, out dropped
      );

      if (success) {
        stored.SetForbidden(true, false);
      }

      return true;
    }

    #endregion


    #region UI

    private int StoredCount {
      get {
        return this.container.Contents.Count;
      }
    }

    public override IEnumerable<Gizmo> GetWornGizmos()  {
      foreach (var g in base.GetWornGizmos()) {
        yield return g;
      }

      if (this.wearer.Downed) {
        yield break;
      }

      if (!IsFull && CanStoreThing(this.wearer.equipment.Primary)) {
        yield return StorePrimaryGizmo;
      }

      foreach (var t in this.container) {
        yield return EquipPrimaryGizmo(t);
      }
    }

    public bool IsEmpty {
      get {
        return StoredCount <= 0;
      }
    }

    public bool IsFull {
      get {
        return StoredCount >= Def.capacity;
      }
    }

    public override IEnumerable<Gizmo> GetGizmos()  {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      if (this.wearer == null && this.SpawnedInWorld) {
        foreach (var t in this.container) {
          yield return RemoveStoredGizmo(t);
        }
      }
    }

    private Gizmo EquipPrimaryGizmo(Thing thing) {
      var equipPrimaryGizmo = new Command_Action();
      equipPrimaryGizmo.action = () => EquipPrimary(thing);
      equipPrimaryGizmo.activateSound = SoundDef.Named("Click");
      equipPrimaryGizmo.icon = Buttons.Icon(this, "EquipStored");
     
      equipPrimaryGizmo.defaultLabel = Labels.ForTitleBrief(thing);
      equipPrimaryGizmo.defaultDesc = Def.equipText
        + " " + this.wearer.Nickname + "'s "
        + Labels.ForSentenceBrief(thing) 
        + " from the " + Labels.ForSentenceBrief(thing) + ".";

      return equipPrimaryGizmo;
    }
      
    private Gizmo StorePrimaryGizmo {
      get {
        if (this.storePrimaryGizmo == null) {
          this.storePrimaryGizmo = new Command_Action();
          this.storePrimaryGizmo.action = () => StorePrimary();
          this.storePrimaryGizmo.activateSound = SoundDef.Named("Click");
          this.storePrimaryGizmo.icon = Buttons.Icon(this, "StorePrimary");
        }

        this.storePrimaryGizmo.defaultLabel = Labels.ForTitleBrief(this);

        this.storePrimaryGizmo.defaultDesc = Def.storeText 
          + " " + this.wearer.Nickname + "'s "
          + Labels.ForSentenceBrief(this.wearer.equipment.Primary)
          + " with the " + Labels.ForSentenceBrief(this) + ".";

        return this.storePrimaryGizmo;
      }
    }

    private Gizmo RemoveStoredGizmo(Thing thing) {
      var removeStoredGizmo = new Command_Action();
      removeStoredGizmo.action = () => RemoveStored(thing);
      removeStoredGizmo.activateSound = SoundDef.Named("Click");
      removeStoredGizmo.icon = Buttons.Icon(this, "RemoveStored");

      removeStoredGizmo.defaultLabel = Labels.ForTitleBrief(thing);

      removeStoredGizmo.defaultDesc = "Remove "
        + Labels.ForSentenceBrief(thing) 
        + " from the " + Labels.ForSentenceBrief(this) + ".";

      return removeStoredGizmo;
    }

    public override string Label {
      get {
        string label;

        label = Labels.ForTitleFull(this);

        if (StoredCount == 1) {
          // FIXME Inefficient (called very frequently)-- though base.Label is 
          // far worse.
          return label + " and " 
            + Labels.ForTitleBrief(this.container.Contents.First());
        }

        return label;
      }
    }

    #endregion

  }
}
