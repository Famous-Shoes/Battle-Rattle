using BattleRattle;

public static class BattleRattle_BattleFieldMedicine_Research {

  public static void Done() {
    Recipes.Inject("TableTailor",
      "BattleRattle_Rucks_Medic_Recipe",
      "BattleRattle_Pouches_IFAK_Recipe"
    );

    Recipes.Inject("TableSmithing", "BattleRattle_Miscellaneous_Buckles_Recipe");

    BattleRattleCompatibility.Instance.ResearchDone(
      "BattleRattle_BattleFieldMedicine_Research"
    );
  }
}
