using BattleRattle;


public static class BattleRattle_Pouches_ResearchPouches {

  public static void Done() {
    Recipes.Inject("TableTailor", "BattleRattle_Pouches_Grenade_Frag_Recipe");
    Recipes.Inject("TableSmithing", "BattleRattle_Miscellaneous_Buckles_Recipe");

    BattleRattleCompatibility.Instance.ResearchDone(
      "BattleRattle_Pouches_ResearchPouches"
    );
  }

}
