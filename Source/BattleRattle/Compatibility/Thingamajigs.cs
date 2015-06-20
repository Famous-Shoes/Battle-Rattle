using Verse;


namespace BattleRattle.Compatibility {
  public class Thingamjigs: ICompatibility {

    public void Inject(InstalledMod mod, InstalledMod battleRattle) {}

    public void ResearchDone(string researchDefName) {
      if (researchDefName == "BattleRattle_ToolCarriers_Research") {
        Recipes.Inject("TableTailor",
          "BattleRattle_ToolCarriers_ToolBelt_Recipe"
        );
      }
    }

  }
}
