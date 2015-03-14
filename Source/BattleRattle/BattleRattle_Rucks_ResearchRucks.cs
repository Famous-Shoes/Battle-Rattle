using BattleRattle;

public static class BattleRattle_Rucks_ResearchRucks {

  public static void Done() {
    Recipes.Inject("TableTailor", 
      "BattleRattle_Rucks_Assault_Recipe",
      "BattleRattle_Rucks_LongRange_Recipe"
    );

    Recipes.Inject("TableSmithing", "BattleRattle_Miscellaneous_Buckles_Recipe");
  }
}
