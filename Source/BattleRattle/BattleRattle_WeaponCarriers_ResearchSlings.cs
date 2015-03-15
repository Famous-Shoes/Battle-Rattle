using BattleRattle;


public static class BattleRattle_WeaponCarriers_ResearchSlings {

  public static void Done() {
    Recipes.Inject("TableTailor", 
      "BattleRattle_WeaponCarriers_SlingHeavy_Recipe",
      "BattleRattle_WeaponCarriers_SlingLight_Recipe"
    );

    Recipes.Inject("TableSmithing", "BattleRattle_Miscellaneous_Buckles_Recipe");

    BattleRattleCompatibility.Instance.ResearchDone(
      "BattleRattle_WeaponCarriers_ResearchSlings"
    );
  }

}
