using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

using RimWorld;
using Verse;

using BattleRattle.Apparel;
using BattleRattle.Utility;


namespace BattleRattle.WeaponCarriers {
  public class WeaponCarrier: AbstractApparel {
  
    private Regex storableRegex;

    private Command_Action storePrimaryGizmo;
    private Command_Action equipPrimaryGizmo;
    private Command_Action removeStoredGizmo;

    private ThingContainer container;
    private Equipment stored;
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
      Scribe_References.LookReference(ref this.stored, "stored");
    }

    #endregion


    #region Storing

    public virtual bool CanStoreThing(Thing thing) {
      return thing != null && StorableRegex.IsMatch(thing.def.label);
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

    public Equipment Stored { 
      get {
        return this.stored;
      }

      set {
        this.stored = value;
      }
    }

    #endregion


    #region Actions

    public virtual bool StorePrimary() {
      Equipment transferred;
      Stored = this.wearer.equipment.Primary;

      var success = this.wearer.equipment.TryTransferEquipmentToContainer(
        Stored, this.container, out transferred
      );

      if (!success) {
        Stored = null;
        Log.Warning(
          "Unable to store primary (" + Stored 
          + ") in carrier's container (" + this.container + ")."
        );
      }

      return true;
    }

    public virtual bool EquipPrimary() {
      Equipment dropped;
      if (this.wearer.equipment.Primary != null) {
        var success = this.wearer.equipment.TryDropEquipment(
          this.wearer.equipment.Primary, out dropped, this.wearer.Position
        );

        if (!success) {
          Messages.Message(
            this.wearer.Nickname + " cannot equip "
            + Labels.ForSentenceBrief(Stored) + " because they have " 
            + Labels.ForSentenceBrief(this.wearer.equipment.Primary)
            + " equipped and no place to drop that."
          );
          return false;
        }
      }

      this.container.Remove(Stored);
      this.wearer.equipment.AddEquipment(Stored);
      Stored = null;

      return true;
    }

    public bool RemoveStored() {
      if (!this.SpawnedInWorld) {
        return false;
      }

      Thing dropped;
      var success = this.container.TryDrop(
        Stored, this.Position, ThingPlaceMode.Near, out dropped
      );

      if (success) {
        GenForbid.SetForbidden(Stored, true, false);
        Stored = null;
      }

      return true;
    }

    #endregion


    #region UI

    public override IEnumerable<Gizmo> GetWornGizmos()  {
      foreach (var g in base.GetWornGizmos()) {
        yield return g;
      }

      if (this.wearer.Downed) {
        yield break;
      }

      if (Stored == null && this.CanStoreThing(this.wearer.equipment.Primary)) {
        yield return StorePrimaryGizmo;
      }

      if (Stored != null) {
        yield return EquipPrimaryGizmo;
      }
    }

    public override IEnumerable<Gizmo> GetGizmos()  {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      if (this.wearer == null && Stored != null && this.SpawnedInWorld) {
        yield return RemoveStoredGizmo;
      }
    }

    private Gizmo EquipPrimaryGizmo {
      get {
        if (this.equipPrimaryGizmo == null) {
          this.equipPrimaryGizmo = new Command_Action();
          this.equipPrimaryGizmo.action = () => EquipPrimary();
          this.equipPrimaryGizmo.activateSound = SoundDef.Named("Click");
          this.equipPrimaryGizmo.icon = Buttons.Icon(this, "EquipStored");
        }

        this.equipPrimaryGizmo.defaultLabel = Def.equipText
          + " " + Labels.ForTitleBrief(Stored);
        this.equipPrimaryGizmo.defaultDesc = Def.equipText
          + " " + this.wearer.Nickname + "'s "
          + Labels.ForSentenceBrief(Stored) + ".";

        return this.equipPrimaryGizmo;
      }
    }
      
    private Gizmo StorePrimaryGizmo {
      get {
        if (this.storePrimaryGizmo == null) {
          this.storePrimaryGizmo = new Command_Action();
          this.storePrimaryGizmo.action = () => StorePrimary();
          this.storePrimaryGizmo.activateSound = SoundDef.Named("Click");
          this.storePrimaryGizmo.icon = Buttons.Icon(this, "StorePrimary");
        }

        this.storePrimaryGizmo.defaultLabel = Def.storeText
          + " " + Labels.ForTitleBrief(this.wearer.equipment.Primary);

        this.storePrimaryGizmo.defaultDesc = Def.storeText 
          + " " + this.wearer.Nickname + "'s "
          + Labels.ForSentenceBrief(this.wearer.equipment.Primary) + ".";

        return this.storePrimaryGizmo;
      }
    }

    private Gizmo RemoveStoredGizmo {
      get {
        if (this.removeStoredGizmo == null) {
          this.removeStoredGizmo = new Command_Action();
          this.removeStoredGizmo.action = () => RemoveStored();
          this.removeStoredGizmo.activateSound = SoundDef.Named("Click");
          this.removeStoredGizmo.icon = Buttons.Icon(this, "RemoveStored");
        }

        this.removeStoredGizmo.defaultLabel = "Remove "
          + " " + Labels.ForTitleBrief(Stored);
        this.removeStoredGizmo.defaultDesc = "Remove "
          + Labels.ForSentenceBrief(Stored) 
          + " from the " + Labels.ForSentenceBrief(this) + ".";

        return this.removeStoredGizmo;
      }
    }

    public override string Label {
      get {
        string label;

        label = Labels.ForTitleFull(this);

        if (Stored != null) {
          // FIXME Inefficient (called very frequently)-- though base.Label is 
          // far worse.
          return label + " and " + Labels.ForTitleBrief(Stored);
        }

        return label;
      }
    }

    #endregion

  }
}
