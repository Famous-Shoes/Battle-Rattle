using Verse;
using RimWorld;


namespace BattleRattle.Compatibility {
  public class Glassworks {
    public void Inject() {
      var medicsTableDef = ThingDef.Named(
        "BattleRattle_BattleFieldMedicine_TableMedical"
      );

      var glassy = DefDatabase<StuffCategoryDef>.GetNamedSilentFail("Glassy");
      if (glassy != null) {
        medicsTableDef.stuffCategories.Add(glassy);
      }
    }
  }
}
