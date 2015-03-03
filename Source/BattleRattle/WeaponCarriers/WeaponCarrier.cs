using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

using RimWorld;
using Verse;

using BattleRattle.Apparel;
using BattleRattle.Things;


namespace BattleRattle.WeaponCarriers {
  public class WeaponCarrier: AbstractApparel {
  
    public WeaponCarrierDef CarrierDef {get; set;}

    private Regex storableRegex;

    private Command_Action storePrimaryGizmo;
    private Command_Action equipPrimaryGizmo;
    private Command_Action removeStoredGizmo;

    private ThingContainer container;
    private Equipment stored;


    #region Lifecycle

    public override void PostMake() {
      base.PostMake();

      this.container = new ThingContainer();
      CarrierDef = (WeaponCarrierDef) this.def;

      this.storableRegex = new Regex(
        CarrierDef.storablePattern, RegexOptions.IgnoreCase
      );
    }

    #endregion


    #region Storing

    public virtual bool CanStoreThing(Thing thing) {
      return thing != null && this.storableRegex.IsMatch(thing.def.label);
    }

    #endregion


    #region Actions

    public virtual bool StorePrimary() {
      Equipment transferred;
      this.stored = this.wearer.equipment.Primary;

      var success = this.wearer.equipment.TryTransferEquipmentToContainer(
        this.stored, this.container, out transferred
      );

      if (!success) {
        this.stored = null;
        Log.Warning(
          "Unable to store primary (" + this.stored 
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
            + Labels.ForSentenceBrief(this.stored) + " because they have " 
            + Labels.ForSentenceBrief(this.wearer.equipment.Primary)
            + " equipped and no place to drop that."
          );
          return false;
        }
      }

      this.container.Remove(this.stored);
      this.wearer.equipment.AddEquipment(this.stored);
      this.stored = null;

      return true;
    }

    public bool RemoveStored() {
      if (!this.SpawnedInWorld) {
        return false;
      }

      Thing dropped;
      var success = this.container.TryDrop(
        this.stored, this.Position, ThingPlaceMode.Near, out dropped
      );

      if (success) {
        GenForbid.SetForbidden(this.stored, true, false);
        this.stored = null;
      }

      return true;
    }

    #endregion


    #region UI

    public override IEnumerable<Gizmo> GetWornGizmos()  {
      foreach (var g in base.GetWornGizmos()) {
        yield return g;
      }

      if (this.stored == null && this.CanStoreThing(this.wearer.equipment.Primary)) {
        yield return StorePrimaryGizmo;
      }

      if (this.stored != null) {
        yield return EquipPrimaryGizmo;
      }
    }

    public override IEnumerable<Gizmo> GetGizmos()  {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      if (this.wearer == null && this.stored != null && this.SpawnedInWorld) {
        yield return RemoveStoredGizmo;
      }
    }

    private Gizmo EquipPrimaryGizmo {
      get {
        if (this.equipPrimaryGizmo == null) {
          this.equipPrimaryGizmo = new Command_Action();
          this.equipPrimaryGizmo.action = () => EquipPrimary();
          this.equipPrimaryGizmo.activateSound = SoundDef.Named("Click");
          this.equipPrimaryGizmo.icon = ButtonIcon("EquipStored");
        }

        this.equipPrimaryGizmo.defaultDesc = CarrierDef.equipText
          + " " + this.wearer.Nickname + "'s "
          + Labels.ForSentenceBrief(this.stored) + ".";

        return this.equipPrimaryGizmo;
      }
    }
      
    private Gizmo StorePrimaryGizmo {
      get {
        if (this.storePrimaryGizmo == null) {
          this.storePrimaryGizmo = new Command_Action();
          this.storePrimaryGizmo.action = () => StorePrimary();
          this.storePrimaryGizmo.activateSound = SoundDef.Named("Click");
          this.storePrimaryGizmo.icon = ButtonIcon("StorePrimary");
        }

        this.storePrimaryGizmo.defaultDesc = CarrierDef.storeText 
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
          this.removeStoredGizmo.icon = ButtonIcon("RemoveStored");
        }

        this.removeStoredGizmo.defaultDesc = "Remove "
          + Labels.ForSentenceBrief(this.stored) 
          + " from the " + Labels.ForSentenceBrief(this) + ".";

        return this.removeStoredGizmo;
      }
    }

    private Texture2D ButtonIcon(string name) {
      return ContentFinder<Texture2D>.Get(
        this.def.defName.Replace("_", "/") + "/Button_" + name, true
      );
    }

    public override string Label {
      get {
        string label;

        label = Labels.ForTitleFull(this);

        if (this.stored != null) {
          // FIXME Inefficient (called very frequently)-- though base.Label is 
          // far worse.
          return label + " and " + Labels.ForTitleBrief(this.stored);
        }

        return label;
      }
    }

    #endregion

  }
}
