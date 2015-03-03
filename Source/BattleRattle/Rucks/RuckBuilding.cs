using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using RimWorld;
using Verse;
using Verse.AI;

using BattleRattle.Things;
using System.Text;

namespace BattleRattle.Rucks {
  public class RuckBuilding: Building_Storage, IRuck {

    public enum RuckIs { Closed, Packing, Unpacking }

    public RuckApparel Apparel {get; set;}

    public int Capacity {get; set;}
    public int CapacityUsed {get; set;}

    public static int FULL_CELL_MULT = 1000000;

    // No generic available in 3.5
    public IList Storage {get; set;}

    private static Texture2D CLOSE_ICON;
    private static Texture2D CLOSE_DISABLED_ICON;
    private static SoundDef CLOSE_SOUND;

    private static Texture2D PACK_ICON;
    private static Texture2D PACK_DISABLED_ICON;
    private static SoundDef PACK_SOUND;

    private static Texture2D UNPACK_ICON;
    private static Texture2D UNPACK_DISABLED_ICON;
    private static SoundDef UNPACK_SOUND;

    private RuckIs mode = RuckIs.Closed;
    private Thing unpacking = null;


    #region Lifecycle

    public override void PostMake() {
      base.PostMake();

      if (CLOSE_ICON == null) {
        var root = this.def.defName.Replace("_Building", "").Replace("_", "/") + "/Button_";
        CLOSE_ICON = ContentFinder<Texture2D>.Get(root + "Close", true);
        CLOSE_DISABLED_ICON = ContentFinder<Texture2D>.Get(root + "Closed", true);
        CLOSE_SOUND = SoundDef.Named("Click");

        PACK_ICON = ContentFinder<Texture2D>.Get(root + "Pack", true);
        PACK_DISABLED_ICON = ContentFinder<Texture2D>.Get(root + "Packing", true);
        PACK_SOUND = SoundDef.Named("Click");

        UNPACK_ICON = ContentFinder<Texture2D>.Get(root + "Unpack", true);
        UNPACK_DISABLED_ICON = ContentFinder<Texture2D>.Get(root + "Unpacking", true);
        UNPACK_SOUND = SoundDef.Named("Click");
      }

      this.Storage = new ArrayList();
    }

    private SpecialThingFilterDef closedFilter;
    private SpecialThingFilterDef openFilter;

    public override void SpawnSetup() {
      base.SpawnSetup();

      Capacity = 600000;
      CapacityUsed = 0;

      Close();
    }

    public override void DeSpawn() {
      base.DeSpawn();
      this.slotGroup.Notify_ParentDestroying();
      this.slotGroup = null;
    }
      
    public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
      base.Destroy(mode);

      foreach (Thing t in this.Storage) {
        if (!t.SpawnedInWorld) {
          t.Destroy(DestroyMode.Vanish);
        }
      }
    }

    #endregion


    #region UI

    public override string GetInspectString() {
      var text = new StringBuilder();
      text.Append(base.GetInspectString());
      if (this.CapacityUsed > 0) {
        text.AppendLine();
        text.AppendLine(this.CapacityPercent() + "% full.");
        foreach (Thing t in this.Storage) {
          text.AppendLine("  » " +  t.stackCount + " " + Labels.ForTitleBrief(t));
        }
      }

      return text.ToString();
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptionsFor(Pawn actor) {
      foreach (var o in base.GetFloatMenuOptionsFor(actor)) {
        yield return o;
      }
        
      var pickup = new Verse.FloatMenuOption();
      pickup.action = () => this.StartPickup(actor);
      pickup.label = "Wear " + this.Label.ToString().ToLower();

      yield return pickup;
    }

    public override IEnumerable<Gizmo> GetGizmos() {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }
        
      var closeAction = new Command_Action();
      closeAction.action = Close;
      closeAction.activateSound = CLOSE_SOUND;
      if (this.mode == RuckIs.Closed) {
        closeAction.icon = CLOSE_DISABLED_ICON;
        closeAction.Disable("Already closed.");
        closeAction.defaultDesc = "The " + Labels.ForSentenceBrief(this) + " is closed.";
        closeAction.defaultLabel = "Closed";

      } else {
        closeAction.icon = CLOSE_ICON;
        closeAction.defaultLabel = "Close";
        closeAction.defaultDesc = "Close this pack so people won't pack or unpack it.";
      }
      yield return closeAction;
        
      var packAction = new Command_Action();
      packAction.action = Pack;
      packAction.activateSound = PACK_SOUND;
      if (this.mode == RuckIs.Packing) {
        packAction.icon = PACK_DISABLED_ICON;
        packAction.Disable("Already packing.");
        packAction.defaultDesc = "This " + Labels.ForSentenceBrief(this) + " is being packed.";
        packAction.defaultLabel = "Packing";

      } else {
        packAction.icon = PACK_ICON;
        packAction.defaultLabel = "Pack";
        packAction.defaultDesc = "Start packing this " + Labels.ForSentenceBrief(this) + ".";
      }
      yield return packAction;

      foreach (Thing t in this.Storage) {
        var unpackAction = new Command_Action();
        unpackAction.action = () => this.Unpack(t);
        unpackAction.defaultDesc = "Switch to packing the " + Labels.ForSentenceBrief(t);
        unpackAction.activateSound = PACK_SOUND;

        if (this.mode == RuckIs.Unpacking && this.unpacking == t) {
          unpackAction.defaultLabel = "Unpacking " + Labels.ForTitleBrief(t);
          unpackAction.icon = UNPACK_DISABLED_ICON;
          unpackAction.Disable("Already unpacking the " + Labels.ForSentenceBrief(t) + ".");
        
        } else {
          unpackAction.defaultLabel = "Unpack " + Labels.ForTitleBrief(t);
          unpackAction.icon = UNPACK_ICON;
        }

        yield return unpackAction;
      }
    }

    #endregion


    #region Hauling

    public override IEnumerable<IntVec3> AllSlotCells() {
      yield return Position;
    }

    #endregion


    #region Actions

    private SpecialThingFilterDef ClosedFilter {
      get {
        if (this.closedFilter == null) {
          this.closedFilter = new SpecialThingFilterDef();
          this.closedFilter.workerClass = typeof(MatchAllFilterWorker);
        }

        return this.closedFilter;
      }
    }

    private SpecialThingFilterDef OpenFilter {
      get {
        if (this.openFilter == null) {
          this.openFilter = new SpecialThingFilterDef();
          this.openFilter.workerClass = typeof(RuckFilterWorker);
          ((RuckFilterWorker) this.openFilter.Worker).Ruck = this;
        }

        return this.openFilter;
      }
    }

    public void Close() {
      GetStoreSettings().allowances.SetAllow(ClosedFilter, false);
      GetStoreSettings().allowances.SetAllow(OpenFilter, true);
      DeSpawnContents();

      this.mode = RuckIs.Closed;
      Log.Message(this + " now closed.");
    }

    public void Pack() {
      GetStoreSettings().allowances.SetAllow(ClosedFilter, true);
      GetStoreSettings().allowances.SetAllow(OpenFilter, false);
      DeSpawnContents();

      this.mode = RuckIs.Packing;
      Log.Message(this + " now packing.");
    }

    public void Unpack(Thing unpack) {
      Close();
      this.unpacking = unpack;
      GenSpawn.Spawn(unpack, this.Position);
//      this.CombinePiles();

      this.mode = RuckIs.Unpacking;
      Log.Message(this + " now unpacking.");
    }
      
    public void DeSpawnContents() {
      foreach (Thing t in this.Storage) {
        if (t.SpawnedInWorld) {
          t.DeSpawn();
        }
      }
    } 

    private void StartPickup(Pawn actor) {
      actor.pather.StartPath(this.Position, Verse.AI.PathMode.Touch);

      if (this.Apparel == null) {
        var pickupDef = ThingDef.Named(this.def.defName.Replace("Building", "Apparel"));
        this.Apparel = (RuckApparel) ThingMaker.MakeThing(pickupDef, this.Stuff);
        Apparel.Building = this;

        Log.Message("No apparel for ruck existed for " + this + ", created " + this.Apparel + ".");
      } else {
        Log.Message("Using existing apparel " + this.Apparel + " for " + this + ".");
      }

      // Apply quality each time, in case the player has changed it.
      // this.apparel.GetComp<RimWorld.CompQuality>().SetQuality(this.GetComp<RimWorld.CompQuality>().Quality);

      // Apply color each time, in case the player has changed it, e.g., with the Snappy Dresser mod.
      this.Apparel.GetComp<CompColorable>().Color = this.GetComp<CompColorable>().Color;

      // Apply health each time as it will degrade overtime
      this.Apparel.Health = this.Health;

      this.FinishPickup(actor);
    }

    public void FinishPickup(Pawn actor) {
      foreach (Thing t in this.Storage) {
        if (t.SpawnedInWorld) {
          t.DeSpawn();
        }
      }

      actor.apparel.Wear(this.Apparel);
      this.DeSpawn();

      Log.Message("Requested " + actor.Nickname + " wear " + this.Apparel + ".");
    }

    #endregion


    #region Events

    public override void Notify_ReceivedThing(Thing newThing) {
      if (newThing == this.Apparel) {
        // This should only happen with the developer's placement tool, but just
        // in case...
        Log.Warning(this + " received the apparel version of itself: " + newThing + ".");
        return;
      } else {
        Log.Message(this + " received " + newThing + ".");
      }

      int storingCount = CanFit(newThing);
      if (storingCount <= 0) {
        Log.Message(this + " (used " + CapacityUsed + " of a capacity of " + Capacity + " ) cannot fit any of " + newThing);
        return;
      }

      Thing storing;
      if (storingCount > newThing.stackCount) {
        Log.Message("Storing whole stack of " + newThing.stackCount + " " + newThing + ".");
        storing = newThing;
        storingCount = newThing.stackCount;
      
      } else {
        storing = newThing.SplitOff(storingCount);
        Log.Message("Storing partial stack of " + storingCount + " " + newThing + ".");
      }

      this.CapacityUsed += storingCount * CapacityUnitFor(storing);
      this.Storage.Add(storing);

      if (this.mode == RuckIs.Packing && storing.SpawnedInWorld) {
        storing.DeSpawn();
      }

      Log.Message(this + " using " + this.CapacityUsed + " of a capacity of " + this.Capacity + ".");
    }

    public override void Notify_LostThing(Thing lostItem) {
      Log.Message(this + " lost " + lostItem + ".");
      this.Storage.Remove(lostItem);
      this.CapacityUsed -= CapacityUnitFor(lostItem) * lostItem.stackCount;
    }

    #endregion


    #region Capacity

    public int CanFit(Thing thing) {
      var remaining = this.Capacity - this.CapacityUsed;
      int fits;

      if (remaining <= 0) {
        Log.Message(this + " is full, cannot fit any of " + thing + ", which has a capacity unit of " + CapacityUnitFor(thing) + "; capacity is " + this.Capacity + " with " + this.CapacityUsed + " used."); 
        return 0;
      }

      fits = remaining / CapacityUnitFor(thing);
      Log.Message(this + " can fit " + fits + " of " + thing + ", which has a capacity unit of " + CapacityUnitFor(thing) + "; capacity is " + this.Capacity + " with " + this.CapacityUsed + " used.");
      return fits;
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

    private int CapacityPercent() {
      return (int) (((this.Capacity - this.CapacityUsed) / (double) this.Capacity) * 100);
    }

    #endregion


    private void CombinePiles() {
      var found = new HashSet<Thing>();
     
      foreach (Thing t in Find.ListerThings.AllThings.FindAll(t => t.Position == this.Position)) {
        found.Add(t);
      }

      foreach(Thing t in found) {
        foreach (Thing o in found) {
          var absorbed = t.TryAbsorbStack(o, true);
          if (absorbed) {
            found.Remove(o);
          }
        }
      }
    }

  }
}
