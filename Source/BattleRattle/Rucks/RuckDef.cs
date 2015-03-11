using System;
using Verse;
using System.Collections.Generic;

namespace BattleRattle.Rucks {
  public class RuckDef : ThingDef {

    public int capacity;
    public int packRadius;

    public List<string> designedForCategories;
    public float designCapacityMultiplier;

    public static RuckDef Instance(string kind) {
      return (RuckDef) ThingDef.Named("BattleRattle_Rucks_" + kind);
    }

  }
}
