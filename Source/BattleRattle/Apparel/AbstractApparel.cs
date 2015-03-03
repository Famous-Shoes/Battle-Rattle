using Verse;

namespace BattleRattle.Apparel {
  public abstract class AbstractApparel: RimWorld.Apparel, IApparel {

    public bool IsWorn { get; set; }

    public override void SpawnSetup() {
      base.SpawnSetup();

      if (this.wearer == null) {
        this.IsWorn = false;
        this.OnDropped();

      } else {
        this.IsWorn = true;
        this.OnWorn();
      }
    }

    public virtual void OnWorn() {
      Log.Message(this.Label + " worn by " + this.wearer.Nickname + ". Override to remove this logging.");
    }

    public virtual void OnDropped() {
      Log.Message(this.Label + " dropped. Override to remove this logging.");
    }

  }
}
