using System;
using System.Collections;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

using UnityEngine;

using BattleRattle.Apparel;
using BattleRattle.Utility;
using System.Text;

namespace BattleRattle.Rucks {

  class Ruck: AbstractApparel, IRuck, ThingContainerGiver {

    enum Mode {
      Closed, Packing, Unpacking
    }

    public ThingFilter PackableCurrent {get; set;}
    public ThingFilter PackableAll {get; set;}
    public int PackRadius {get; set;}

    private Command_Action closeGizmo;
    private Command_Action closeDisabledGizmo;
    private Command_Action packGizmo;
    private Command_Action packDisabledGizmo;

    private Mode mode;
    private int capacityUsed;
    private ThingContainer container;

    public static int FULL_CELL_MULT = 1000000;


    public RuckDef Def {
      get {
        return (RuckDef) this.def;
      }
    }


    #region Lifecycle

    public override void PostMake() {
      base.PostMake();

      PackRadius = Math.Min(10, this.Def.packRadius);

      PackableAll = new ThingFilter();
      PackableAll.SetAllow(ThingCategoryDef.Named("Apparel"), true);
      PackableAll.SetAllow(ThingCategoryDef.Named("Art"), true);
      PackableAll.SetAllow(ThingCategoryDef.Named("BodyParts"), true);
      PackableAll.SetAllow(ThingCategoryDef.Named("Chunks"), true);
      PackableAll.SetAllow(ThingCategoryDef.Named("Corpses"), true);
      PackableAll.SetAllow(ThingCategoryDef.Named("Items"), true);
      PackableAll.SetAllow(ThingCategoryDef.Named("Resources"), true);
      PackableAll.SetAllow(ThingCategoryDef.Named("Weapons"), true);
      PackableAll.ResolveReferences();

      PackableCurrent = new ThingFilter();
      PackableCurrent.DisallowAll();
      if (this.Def.designedFor != null) {
        foreach (var defName in this.Def.designedFor) {
          PackableCurrent.SetAllow(ThingDef.Named(defName), true);
        }
      }
      PackableCurrent.ResolveReferences();

      this.container = new ThingContainer();
      this.capacityUsed = 0;
    }

    public override void SpawnSetup () {
      base.SpawnSetup();

      StopPacking();
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
      base.Destroy(mode);
      this.container.DestroyContents();
    }

    #endregion


    #region Packing

    public void Pack(Pawn packer, Thing packing) {
      var fits = CanFit(packing);

      #if DEBUG
      Log.Message(
        this + " received " + packing.stackCount + " " + packing 
        + ", can fit " + fits + "."
      );
      #endif

      packer.carryHands.GetContainer().TransferToContainer(
        packing, this.container, fits
      );
      this.capacityUsed += CapacityUnitFor(packing) * fits;
    }

    public ThingContainer GetContainer() {
      return this.container;
    }

    public bool IsPackable {
      get {
        return this.mode == Mode.Packing && SpawnedInWorld;
      }
    }

    public int Capacity {
      get {
        var quality = QualityCategory.Awful;
        this.TryGetQuality(out quality);
       
        var qualityAdjustment = 1 + ((int) quality * 0.1f);

        return Mathf.RoundToInt(this.Def.capacity * qualityAdjustment);
      }
    }

    public int CanFit(Thing thing) {
      var remaining = this.Capacity - this.capacityUsed;

      if (remaining <= 0) {
        return 0;
      }

      return Math.Max(0, Math.Min(
        thing.stackCount, (int) (remaining / (double) CapacityUnitFor(thing))
      ));
    }

    public bool CheckPackable(Thing thing) {
      if (CanFit(thing) <= 0) {
        #if DEBUG
        Log.Message(this + " has no capacity left; cannot pack " + thing + ".");
        #endif

        return false;
      }

      if (thing == this) {
        Log.Warning(this + " cannot be packed into itself; what's wrong with you?");

        return false;
      }

      if (!this.PackableCurrent.Allows(thing.def)) {
        #if DEBUG
        Log.Message(
          this + " isn't configured to accept " + thing.def 
          + "; cannot pack any " + thing + "."
        );
        #endif

        return false;
      }

      if (Find.Reservations.ReserverOf(thing, Faction.OfColony) != null) {
        #if DEBUG
        Log.Message(thing + " is reserved and cannot be packed into " + this + ".");
        #endif

        return false;
      }

      if (thing.IsForbidden(Faction.OfColony)) {
        #if DEBUG
        Log.Message(thing + " is forbidden and cannot be packed into " + this + ".");
        #endif

        return false;
      }

      if (thing.IsBurning()) {
        #if DEBUG
        Log.Message(thing + " is bloody ON FIRE and cannot be packed into " + this + ".");
        #endif

        return false;
      }

      return true;
    }

    private int CapacityUnitFor(Thing thing) {
      // TODO Just placeholder nonsense. Should calculate based on:
      // Crafted items: ingredient count
      // Resources: stack limit
      // Should also be cached/singletonized or something
      int unit;

      if (thing.def.IsApparel || thing.def.IsMeleeWeapon || thing.def.IsRangedWeapon) {
        unit = 100000;

      } else {
        var limit = (thing.def.stackLimit > 0) ? thing.def.stackLimit : 75;

        unit = FULL_CELL_MULT / limit;
      }

      if (this.Def.designedFor != null) {
        if (this.Def.designedFor.IndexOf(thing.def.defName) != -1) {
          unit *= Mathf.RoundToInt(this.Def.designCapacityMultiplier);
        }
      }

      return unit;
    }

    #endregion


    #region Actions

    public void StartPacking() {
      this.mode = Mode.Packing;
    }

    public void StopPacking() {
      this.mode = Mode.Closed;
    }

    public void Unpack(Thing unpacking) {
      Thing dropped;

      #if DEBUG
      Log.Message("Unpacking " + unpacking + " from " + this + ".");
      #endif

      var success = this.container.TryDrop(
        unpacking, this.Position, ThingPlaceMode.Near, out dropped
      );

      if (success) {
        this.capacityUsed -= CapacityUnitFor(dropped) * dropped.stackCount;
      }
    }

    private void Drop() {
      RimWorld.Apparel droppedApparel;
      this.wearer.apparel.TryDrop(this, out droppedApparel, this.wearer.Position);
    }

    #endregion


    #region UI

    public override string GetInspectString() {
      var text = new StringBuilder();
      text.Append(base.GetInspectString());
      if (this.capacityUsed > 0) {
        var percent = (int) Math.Round(
          (100 / (double) this.Capacity) * this.capacityUsed
        );
        text.AppendLine();
        text.AppendLine(percent + "% full.");
        foreach (Thing t in this.container) {
          text.AppendLine("  » " +  t.stackCount + " " + Labels.ForTitleBrief(t));
        }
      }

      #if DEBUG
      var quality = QualityCategory.Awful;
      this.TryGetQuality(out quality);
      var qualityAdjustment = 1 + ((int) quality * 0.1f);

      text.AppendLine();
      text.AppendLine("Quality: " + (int) quality);
      text.AppendLine("Quality Adjustment for Capacity: " + qualityAdjustment);
      text.AppendLine("Capacity: " + this.Capacity + ", Used: " + this.capacityUsed);
      #endif

      return text.ToString();
    }

    public override IEnumerable<Gizmo> GetGizmos() {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      if (this.mode != Mode.Closed) {
        yield return CloseGizmo;

      } else {
        yield return CloseDisabledGizmo;
      }

      if (this.mode != Mode.Packing) {
        yield return PackGizmo;

      } else {
        yield return PackDisabledGizmo;
      }

      foreach (Thing t in this.container) {
        var unpackGizmo = new Command_Action();
        unpackGizmo.action = () => Unpack(t);
        unpackGizmo.activateSound = SoundDef.Named("Click");
        unpackGizmo.defaultDesc = "Unpack all the " + Labels.ForTitleBrief(t) 
          + " from the " + Labels.ForTitleBrief(this) + ".";
        unpackGizmo.icon = Buttons.Icon(this, "Unpack");
        unpackGizmo.defaultLabel = Labels.ForTitleBrief(t);

        #if DEBUG
        Log.Message(
          "Found " + t + " in " + this + "; adding unpack gizmo: " 
          + unpackGizmo + "."
        );
        #endif

        yield return unpackGizmo;
      }
    }

    public override IEnumerable<Gizmo> GetWornGizmos() {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      if (this.wearer.Downed) {
        yield break;
      }

      var drop = new Command_Action();

      drop.action = Drop;
      drop.icon = Buttons.Icon(this, "Drop");
      drop.defaultDesc = "Have " + this.wearer.Nickname + " drop the " 
        + Labels.ForSentenceBrief(this) + ".";
      drop.activateSound = SoundDef.Named("Click");

      yield return drop;
    }

    private Command_Action CloseGizmo {
      get {
        if (this.closeGizmo == null) {
          this.closeGizmo = new Command_Action();
          this.closeGizmo.action = StopPacking;
          this.closeGizmo.activateSound = SoundDef.Named("Click");
          this.closeGizmo.icon = Buttons.Icon(this, "Close");
          this.closeGizmo.defaultDesc = "Close this "
            + Labels.ForSentenceBrief(this) + " so people won't pack"
            + " or unpack it.";
        }

        return this.closeGizmo;
      }
    }

    private Command_Action CloseDisabledGizmo {
      get {
        if (this.closeDisabledGizmo == null) {
          this.closeDisabledGizmo = new Command_Action();
          this.closeDisabledGizmo.icon = Buttons.Icon(this, "CloseDisabled");
          this.closeDisabledGizmo.Disable("Already closed.");
          this.closeDisabledGizmo.defaultDesc = "The "
            + Labels.ForSentenceBrief(this) + " is closed; not being packed.";
        }

        return this.closeDisabledGizmo;
      }
    }

    private Command_Action PackGizmo {
      get {
        if (this.packGizmo == null) {
          this.packGizmo = new Command_Action();
          this.packGizmo.action = StartPacking;
          this.packGizmo.icon = Buttons.Icon(this, "Pack");
          this.packGizmo.defaultDesc = "Start packing the "
            + Labels.ForSentenceBrief(this) + ".";
        }

        return this.packGizmo;
      }
    }

    private Command_Action PackDisabledGizmo {
      get {
        if (this.packDisabledGizmo == null) {
          this.packDisabledGizmo = new Command_Action();
          this.packDisabledGizmo.icon = Buttons.Icon(this, "PackDisabled");
          this.packDisabledGizmo.Disable("Already packing.");
          this.packGizmo.defaultDesc = "The "
            + Labels.ForSentenceBrief(this) + " is being packed.";
        }

        return this.packDisabledGizmo;
      }
    }

    #endregion

  }
}

