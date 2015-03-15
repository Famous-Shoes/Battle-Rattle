using UnityEngine;
using Verse;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using BattleRattle.Compatibility;

namespace BattleRattle {
  public class BattleRattleCompatibility: MonoBehaviour {

    public void Start() {
      var activeMods = InstalledModLister.AllInstalledMods.Where(m => m.Active);
      InstalledMod battleRattle = null;
      int installationsCount = 0;

      foreach (var mod in activeMods) {
        if (mod.Name == "Battle Rattle") {
          battleRattle = mod;
          installationsCount++;
        }
      }

      if (battleRattle == null) {
        // Loaded but not activated.
        return;
      }

      if (installationsCount > 1) {
        Log.Error("Multiple installations of Battle Rattle found; this won't end well.");

      } else {
        // In theory a Class attribute on the ModMetaData element would work 
        // here, but when BattleRattle is inactive, RimWorld won't load the DLL 
        // and so will throw an exception when the user opens the mod listing 
        // dialogue.

        var metaData = XmlLoader.ItemFromXmlFile<BattleRattleMetaData>(
          battleRattle.dir + "/About/About.xml", false
        );

        Log.Message(
          metaData.name + " " + metaData.version 
          + " checking for compatible and incompatible mods:"
        );
      }

      foreach (var mod in activeMods) {
        if (mod.Name == "Better Than Sentry Guns") {
          Log.Message(" + Injecting compatibility for " + mod.Name + ".");
          new BetterThanSentryGuns().Inject();

        } else if (mod.Name == "[T] ExpandedCloth") {
          Log.Warning(" - Compatibility for Expanded Cloth not yet implemented.");

        } else if (Regex.IsMatch(mod.Name, "Extended Woodworking")) {
          Log.Warning(" - Compatibility for Extended Woodworking not yet implemented.");

        } else if (Regex.IsMatch(mod.Name, "Glassworks")) {
          Log.Message(" + Injecting compatibility for " + mod.Name + ".");
          new Glassworks().Inject();

        } else if (Regex.IsMatch(mod.Name, "NonLethals")) {
          Log.Warning(" - Compatibility for Non-Lethals not yet implemented.");

        } else if (Regex.IsMatch(mod.Name, "Right Tool For The Job")) {
          Log.Warning(" - Compatibility for The Right Tool for the Job not yet implemented.");
        
        } else if (mod.Name == "Rimsearch") {
          Log.Warning(" - Compatibility for Rimsearch not yet implemented.");
        
        } else if (mod.Name == "Winter Is Here") {
          Log.Warning(" - Compatibility for Winter is Here not yet implemented.");
        }
      }

      Log.Message(" * Compatibility checking complete.");
    }

  }
}
