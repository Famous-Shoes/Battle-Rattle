using System.Collections.Generic;

using Verse;


namespace BattleRattle {
  public static class Recipes {

    public static void Inject(string tableName, params string[] recipeNames) {
      var table = DefDatabase<ThingDef>.GetNamed(tableName);

      foreach (var name in recipeNames) {
        var recipe = DefDatabase<RecipeDef>.GetNamed(name);

        if (recipe == null) {
          Log.Error("Missing recipe named '" + name + "'.");
          continue;
        }

        if (!table.AllRecipes.Contains(recipe)) {
          #if DEBUG
          Log.Message(
            "Adding recipe " + recipe + " to table " + table + "."
          );
          #endif

          table.AllRecipes.Add(recipe);
        }
      }
    }

  }
}
