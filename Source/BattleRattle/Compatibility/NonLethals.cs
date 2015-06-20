using Verse;


namespace BattleRattle.Compatibility {
  public class NonLethals: ICompatibility {


    public void Inject(InstalledMod mod, InstalledMod battleRattle) {}

    public void ResearchDone(string researchDefName) {
      if (researchDefName == "BattleRattle_WeaponCarriers_ResearchHolsters") {
        Recipes.Inject("TableTailor", 
          "BattleRattle_WeaponCarriers_NonLethals_StunGun_Recipe",
          "BattleRattle_WeaponCarriers_NonLethals_Tazer_Recipe"
        );
      }
    }

  }
}
