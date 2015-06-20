using System;

using Verse;


namespace BattleRattle.Compatibility {
  public class Rimsearch: ICompatibility {

    public void Inject(InstalledMod mod, InstalledMod battleRattle) {
      var researchNames = new [] {
        "BattleRattle_Rucks_ResearchBasic",
        "BattleRattle_Rucks_ResearchAdvanced",
        "BattleRattle_Pouches_ResearchPouches",
        "BattleRattle_ToolCarriers_Research",
        "BattleRattle_WeaponCarriers_ResearchHolsters",
        "BattleRattle_WeaponCarriers_ResearchSheathes",
        "BattleRattle_WeaponCarriers_ResearchSlings",
        "BattleRattle_BattleFieldMedicine_Research"
      };

      foreach (var n in researchNames) {
        DefDatabase<ResearchProjectDef>.GetNamed(n).totalCost *= 3;
      }
    }

    public void ResearchDone(string researchDefName) {}

  }
}
