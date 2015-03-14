using Verse;

namespace BattleRattle.Rucks {
  public class PackRuck_WorkGiverDef: WorkGiverDef {
   
    public string ruckDefName;

    public ThingDef RuckDef {
      get {
        return ThingDef.Named(this.ruckDefName);
      }
    }

  }
}

