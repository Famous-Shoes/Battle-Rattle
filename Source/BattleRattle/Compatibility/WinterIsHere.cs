using System;
using System.Linq;
using Verse;

namespace BattleRattle.Compatibility {
  public class WinterIsHere: ICompatibility {

    public void Inject() {}

    public void ResearchDone(string researchDefName) {
      var tailor = DefDatabase<ThingDef>.GetNamed("TableTailor");
      var kitTailor = DefDatabase<ThingDef>.GetNamed("TableTailor_FromKit");
      var tailorRecipes = tailor.AllRecipes.Where(r => r.defName.StartsWith("BattleRattle"));
      foreach (var r in tailorRecipes) {
        if (!kitTailor.AllRecipes.Contains(r)) {
          kitTailor.AllRecipes.Add(r);
        }
      }

      var smith = DefDatabase<ThingDef>.GetNamed("TableSmithing");
      var kitSmith = DefDatabase<ThingDef>.GetNamed("TableSmithing_FromKit");
      var smithRecipes = smith.AllRecipes.Where(r => r.defName.StartsWith("BattleRattle"));
      foreach (var r in smithRecipes) {
        if (!kitSmith.AllRecipes.Contains(r)) {
          kitSmith.AllRecipes.Add(r);
        }
      }
    }

  }
}
