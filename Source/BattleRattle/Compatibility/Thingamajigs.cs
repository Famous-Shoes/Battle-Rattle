using Verse;
using RimWorld;


namespace BattleRattle.Compatibility {
  public class Thingamjigs: ICompatibility {

    public void Inject() {}

    public void ResearchDone(string researchDefName) {
      if (researchDefName == "BattleRattle_ToolCarriers_Research") {
        Recipes.Inject("TableTailor",
          "BattleRattle_ToolCarriers_ToolBelt_Recipe"
        );
      }
    }

  }
}
