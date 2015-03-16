using Verse;
using RimWorld;


namespace BattleRattle.Compatibility {
  public class RightToolForTheJob: ICompatibility {

    public void Inject() {}

    public void ResearchDone(string researchDefName) {
      Recipes.Inject("TableTailor",
        "BattleRattle_ToolCarriers_ToolBelt_Recipe"
      );
    }

  }
}
