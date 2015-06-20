using RimWorld;
using Verse;


namespace BattleRattle.Compatibility {
  public class Glassworks: ICompatibility {

    public void Inject(InstalledMod mod, InstalledMod battleRattle) {
      var medicsTableDef = ThingDef.Named(
        "BattleRattle_BattleFieldMedicine_TableMedical"
      );

      var glassy = DefDatabase<StuffCategoryDef>.GetNamedSilentFail("Glassy");
      if (glassy != null) {
        medicsTableDef.stuffCategories.Add(glassy);
      }
    }

    public void ResearchDone(string researchDefName) {}

  }
}
