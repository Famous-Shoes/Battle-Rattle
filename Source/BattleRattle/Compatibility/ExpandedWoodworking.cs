using System.Linq;

using Verse;


namespace BattleRattle.Compatibility {
  public class ExpandedWoodworking: ICompatibility {

    public void Inject(InstalledMod mod, InstalledMod battleRattle) {
      var woodRecipeNames = new string[] {
        "BattleRattle_WeaponCarriers_Scabbard_Recipe",
        "BattleRattle_WeaponCarriers_Sheath_Recipe"
      };

      var woodTypesDef = ThingCategoryDef.Named("WoodTypes");

      foreach (var n in woodRecipeNames) {
        var recipe = DefDatabase<RecipeDef>.GetNamed(n);
        if (recipe == null) {
          Log.Error("Missing recipe " + n + ".");
        
        } else {
          var filters = new ThingFilter[] {
            recipe.defaultIngredientFilter,
            recipe.fixedIngredientFilter,
            recipe.ingredients.First().filter
          };

          foreach (var f in filters) {
            f.SetAllow(woodTypesDef, true);
          }
        }
      }
    }

    public void ResearchDone(string researchDefName) {}

  }
}
