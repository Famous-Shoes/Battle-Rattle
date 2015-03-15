using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;


namespace BattleRattle.Compatibility {
  public class ExpandedWoodworking: ICompatibility {

    public void Inject() {     
      var woodRecipeNames = new string[] {
        "BattleRattle_WeaponCarriers_Scabbard_Recipe",
        "BattleRattle_WeaponCarriers_Sheath_Recipe"
      };

      foreach (var n in woodRecipeNames) {
        var recipe = DefDatabase<RecipeDef>.GetNamed(n);
        if (recipe == null) {
          Log.Error("Missing recipe " + n + ".");
        
        } else {
          if (recipe.defaultIngredientFilter.categories == null) {
            recipe.defaultIngredientFilter.categories = new List<string>();
          }
          recipe.defaultIngredientFilter.categories.Add("WoodTypes");

          if (recipe.fixedIngredientFilter.categories == null) {
            recipe.fixedIngredientFilter.categories = new List<string>();
          }
          recipe.fixedIngredientFilter.categories.Add("WoodTypes");

          if (recipe.ingredients.First().filter.categories == null) {
            recipe.ingredients.First().filter.categories = new List<string>();
          }
          recipe.ingredients.First().filter.categories.Add("WoodTypes");
        }
      }
    }

    public void ResearchDone(string researchDefName) {}

  }
}
