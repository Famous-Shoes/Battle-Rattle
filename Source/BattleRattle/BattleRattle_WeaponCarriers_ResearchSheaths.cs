using BattleRattle;


public static class BattleRattle_WeaponCarriers_ResearchSheathes {

  public static void Done() {
    Recipes.Inject("TableTailor", 
      "BattleRattle_WeaponCarriers_Scabbard_Recipe",
      "BattleRattle_WeaponCarriers_Sheath_Recipe"
    );

    Recipes.Inject("TableSmithing", "BattleRattle_Miscellaneous_Buckles_Recipe");

    BattleRattleCompatibility.Instance.ResearchDone(
      "BattleRattle_WeaponCarriers_ResearchSheathes"
    );
  }

}
