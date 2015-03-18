using System;
using System.Collections.Generic;

using Verse;


namespace BattleRattle.Compatibility {
  public class BetterThanSentryGuns: ICompatibility {

    public void Inject() {
      var tailoring = DefDatabase<ResearchProjectDef>.GetNamed("BTSGTailoring");

      var allResearch = new List<ResearchProjectDef>();
      var tailorResearchNames = new [] {
        "BattleRattle_Rucks_ResearchBasic",
        "BattleRattle_Rucks_ResearchAdvanced",
        "BattleRattle_Pouches_ResearchPouches",
        "BattleRattle_ToolCarriers_Research",
        "BattleRattle_WeaponCarriers_ResearchHolsters",
        "BattleRattle_WeaponCarriers_ResearchSheathes",
        "BattleRattle_WeaponCarriers_ResearchSlings"
      };

      var otherResearchNames = new [] {
        "BattleRattle_BattleFieldMedicine_Research",
      };

      foreach (string name in tailorResearchNames) {
        var project = DefDatabase<ResearchProjectDef>.GetNamed(name);
        project.prerequisites.Add(tailoring);
        allResearch.Add(project);
      }

      foreach (string name in otherResearchNames) {
        var project = DefDatabase<ResearchProjectDef>.GetNamed(name);
        allResearch.Add(project);
      }

      foreach (ResearchProjectDef project in allResearch) {
        project.totalCost *= 2;
      }
    }

    public void ResearchDone(string researchDefName) {}

  }
}
