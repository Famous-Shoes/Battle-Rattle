using System;
using System.Collections.Generic;

using Verse;


namespace BattleRattle.Compatibility {
  public class Rimsearch: ICompatibility {

    public void Inject() {
      var researchNames = new [] {
        "BattleRattle_Rucks_ResearchBasic",
        "BattleRattle_Rucks_ResearchAdvanced",
        "BattleRattle_Pouches_Research",
        "BattleRattle_WeaponCarriers_Holster_Research",
        "BattleRattle_WeaponCarriers_Sheaths_Research",
        "BattleRattle_BattleFieldMedicine_Research",
        "BattleRattle_WeaponCarriers_Slings_Research"
      };

      foreach (var n in researchNames) {
        DefDatabase<ResearchProjectDef>.GetNamed(n).totalCost *= 3;
      }
    }

    public void ResearchDone(string researchDefName) {}

  }
}
