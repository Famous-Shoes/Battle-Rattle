using System;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace BattleRattle.Rucks {
  public class RuckDef : ThingDef {

    public int capacity;
    public int packRadius;

    public List<string> designedFor;
    public float designCapacityMultiplier;

    public static RuckDef Instance(string kind) {
      return (RuckDef) ThingDef.Named("BattleRattle_Rucks_" + kind);
    }

//    public override IEnumerable<RimWorld.StatDrawEntry> SpecialDisplayStats {
//      get {
//        foreach (var s in base.SpecialDisplayStats) {
//          return yield s;
//        }
//
//        var stat = new StatDrawEntry(StatCategoryDefOf.Basics, "Capacity", string.Empty, 99999);
//        stat.overrideReportText = "XYZ";
//      }
//    }
  }
}
