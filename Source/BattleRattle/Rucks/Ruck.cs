using System;
using System.Collections;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

using UnityEngine;

using BattleRattle.Apparel;
using BattleRattle.Things;
using System.Text;

namespace BattleRattle.Rucks {

  class Ruck: AbstractApparel, IRuck, ThingContainerGiver {

    enum Mode {
      Closed, Packing, Unpacking
    }

    public ThingFilter PackableCurrent {get; set;}
    public ThingFilter PackableAll {get; set;}
    public int PackRadius {get; set;}

    private RuckDef ruckDef;

    private Command_Action closeGizmo;
    private Command_Action closeDisabledGizmo;
    private Command_Action packGizmo;
    private Command_Action packDisabledGizmo;

    private Mode mode;
    private int capacityUsed;
    private ThingContainer container;

    public static int FULL_CELL_MULT = 1000000;


    #region Lifecycle

    public override void PostMake() {
      base.PostMake();

      this.ruckDef = (RuckDef) this.def;

      PackRadius = Math.Min(10, this.ruckDef.packRadius);

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

    public int CanFit(Thing thing) {
      var remaining = this.ruckDef.capacity - this.capacityUsed;

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

    private static int CapacityUnitFor(Thing thing) {
      // TODO Just placeholder nonsense. Should calculate based on:
      // Crafted items: ingredient count
      // Resources: stack limit
      // Should also be cached/singletonized or something
      if (thing.def.IsApparel || thing.def.IsMeleeWeapon || thing.def.IsRangedWeapon) {
        return 100000;

      } else {
        var limit = (thing.def.stackLimit > 0) ? thing.def.stackLimit : 75;

        return FULL_CELL_MULT / limit;
      }
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
          (100 / (double) this.ruckDef.capacity) * this.capacityUsed
        );
        text.AppendLine();
        text.AppendLine(percent + "% full.");
        foreach (Thing t in this.container) {
          text.AppendLine("  » " +  t.stackCount + " " + Labels.ForTitleBrief(t));
        }
      }

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

      foreach (var t in this.container) {
        var unpackGizmo = new Command_Action();
        unpackGizmo.action = () => Unpack(t);
        unpackGizmo.activateSound = SoundDef.Named("Click");
        unpackGizmo.defaultLabel = "Unpack " + Labels.ForTitleBrief(t);
        unpackGizmo.defaultDesc = "Unpack all the " + Labels.ForTitleBrief(t) 
          + " from the " + Labels.ForTitleBrief(this) + ".";
        unpackGizmo.icon = ButtonIcon("Unpack");

        yield return unpackGizmo;
      }
    }

    public override IEnumerable<Gizmo> GetWornGizmos() {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      var drop = new Command_Action();

      drop.action = Drop;
      drop.icon = ButtonIcon("Drop");
      drop.defaultLabel = "Drop " + Labels.ForTitleBrief(this);
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
          this.closeGizmo.icon = ButtonIcon("Close");
          this.closeGizmo.defaultLabel = "Close";
          this.closeGizmo.defaultDesc = "Close this pack so people won't pack"
            + " or unpack it.";
        }

        return this.closeGizmo;
      }
    }

    private Command_Action CloseDisabledGizmo {
      get {
        if (this.closeDisabledGizmo == null) {
          this.closeDisabledGizmo = new Command_Action();
          this.closeDisabledGizmo.icon = ButtonIcon("CloseDisabled");
          this.closeDisabledGizmo.Disable("Already closed.");
          this.closeDisabledGizmo.defaultLabel = "Closed";
        }

        return this.closeDisabledGizmo;
      }
    }

    private Command_Action PackGizmo {
      get {
        if (this.packGizmo == null) {
          this.packGizmo = new Command_Action();
          this.packGizmo.action = StartPacking;
          this.packGizmo.icon = ButtonIcon("Pack");
          this.packGizmo.defaultDesc = "Start packing the "
            + Labels.ForSentenceBrief(this) + ".";
          this.packGizmo.defaultLabel = "Pack";
        }

        return this.packGizmo;
      }
    }

    private Command_Action PackDisabledGizmo {
      get {
        if (this.packDisabledGizmo == null) {
          this.packDisabledGizmo = new Command_Action();
          this.packDisabledGizmo.icon = ButtonIcon("PackDisabled");
          this.packDisabledGizmo.Disable("Already packing.");
          this.packDisabledGizmo.defaultLabel = "Packing";
        }

        return this.packDisabledGizmo;
      }
    }

    private Texture2D ButtonIcon(string name) {
      return ContentFinder<Texture2D>.Get(
        this.def.defName.Replace("_", "/") + "/Button_" + name, true
      );
    }

    #endregion

  }
}

