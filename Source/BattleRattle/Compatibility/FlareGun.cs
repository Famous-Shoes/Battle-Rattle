using System;

namespace BattleRattle.Compatibility {
  public class FlareGun: ICompatibility {


    public void Inject() {}

    public void ResearchDone(string researchDefName) {
      if (researchDefName == "BattleRattle_WeaponCarriers_ResearchHolsters") {
        Recipes.Inject("TableTailor", 
          "BattleRattle_WeaponCarriers_Skullywag_FlareGun_Recipe"
        );
      }
    }

  }
}
