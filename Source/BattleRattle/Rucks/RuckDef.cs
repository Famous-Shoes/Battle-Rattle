using System;
using Verse;
using System.Collections.Generic;

namespace BattleRattle.Rucks {
  public class RuckDef : ThingDef {

    public int capacity;
    public int packRadius;

    public List<string> designedForCategories;
    public float designCapacityMultiplier;

  }
}
