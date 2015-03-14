﻿using System;
using Verse;
using System.Collections.Generic;

namespace BattleRattle {
  public class Compatibility_BetterThanSentryGuns {

    public void Inject() {
      var tailoring = DefDatabase<ResearchProjectDef>.GetNamed("BTSGTailoring");

      var allResearch = new List<ResearchProjectDef>();
      var tailorResearchNames = new [] {
        "BattleRattle_Rucks_Research",
        "BattleRattle_Pouches_Research",
        "BattleRattle_WeaponCarriers_Holster_Research",
        "BattleRattle_WeaponCarriers_Sheaths_Research"
      };

      var otherResearchNames = new [] {
        "BattleRattle_BattleFieldMedicine_Research",
        "BattleRattle_WeaponCarriers_Slings_Research"
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

  }
}
