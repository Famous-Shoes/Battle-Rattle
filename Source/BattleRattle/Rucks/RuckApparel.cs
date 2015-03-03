using System.Collections.Generic;

using UnityEngine;

using RimWorld;
using Verse;

using BattleRattle.Apparel;
using BattleRattle.Things;

namespace BattleRattle.Rucks {
  public class RuckApparel: AbstractApparel {

    public RuckBuilding Building {get; set;}

    private static Texture2D DROP_ICON;
    private static SoundDef DROP_SOUND;

    public override void SpawnSetup() {
      base.SpawnSetup();

      if (DROP_ICON == null) {
        DROP_ICON = ContentFinder<Texture2D>.Get("UI/Buttons/Minus", true);
        DROP_SOUND = SoundDef.Named("Click");
      }
    }

    public override IEnumerable<Gizmo> GetWornGizmos()  {
      foreach (var g in base.GetGizmos()) {
        yield return g;
      }

      var drop = new Command_Action();
      drop.action = () => this.Drop();
      drop.icon = DROP_ICON;
      drop.defaultLabel = "Drop Pack";
      drop.defaultDesc = "Drop " + this.wearer.Nickname + "'s " + Labels.ForSentenceBrief(this) + ".";
      drop.activateSound = DROP_SOUND;

      yield return drop;

    }

    private void Drop() {
      RimWorld.Apparel droppedApparel;
      this.wearer.apparel.TryDrop(this, out droppedApparel, this.wearer.Position);
    }

    public override void OnDropped() {
      base.OnDropped();

      if (this.Building == null) {
        var dropDef = ThingDef.Named(this.def.defName.Replace("Apparel", "Building"));
        this.Building = (RuckBuilding) ThingMaker.MakeThing(dropDef, this.Stuff);
        this.Building.Apparel = this;

        Log.Message("No building for ruck existed for " + this + ", created " + this.Building + ".");
      } else {
        Log.Message("Using existing building " + this.Building + " for " + this + ".");
      }

      // Apply quality each time, in case the player has changed it.
      // this.building.GetComp<RimWorld.CompQuality>().SetQuality(this.GetComp<RimWorld.CompQuality>().Quality);

      // Apply color each time, in case the player has changed it, e.g., with the Snappy Dresser mod.
      this.Building.GetComp<CompColorable>().Color = this.GetComp<CompColorable>().Color;

      // Apply health each time as it will degrade overtime
      this.Building.Health = this.Health;

      // Despawn first else the engine will "add" the apparel to the building,
      // i.e., fire Notify_ReceivedThing.
      var targetPosition = this.Position;
      this.DeSpawn();
      GenSpawn.Spawn(this.Building, targetPosition);

      Log.Message("Requested " + this.Building + " spawn at " + this.Position + " for " + this + ".");
    }
 
  }
}
