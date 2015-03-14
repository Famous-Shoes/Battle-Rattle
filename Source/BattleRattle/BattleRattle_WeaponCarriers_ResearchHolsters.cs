using BattleRattle;


public static class BattleRattle_WeaponCarriers_ResearchHolsters {

  public static void Done() {
    Recipes.Inject("TableTailor", 
      "BattleRattle_WeaponCarriers_HolsterDropLeg_Recipe",
      "BattleRattle_WeaponCarriers_HolsterPDW_Recipe",
      "BattleRattle_WeaponCarriers_HolsterShoulder_Recipe"
    );

    Recipes.Inject("TableSmithing", "BattleRattle_Miscellaneous_Buckles_Recipe");
  }
}
