using System;
using Verse;

namespace BattleRattle.BattleFieldMedicine {
  public class TraumaKitDef: ThingDef {

    public int ticksForTreatment;

    public static TraumaKitDef Instance {
      get {
        return (TraumaKitDef) ThingDef.Named(
          "BattleRattle_BattleFieldMedicine_TraumaKit"
        );
      }
    }
  }
}

