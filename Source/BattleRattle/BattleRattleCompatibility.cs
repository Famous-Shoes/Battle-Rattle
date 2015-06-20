using UnityEngine;
using Verse;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using BattleRattle.Compatibility;
using System;
using System.IO;

namespace BattleRattle {
  public class BattleRattleCompatibility: MonoBehaviour {
  
    private List<ICompatibility> listeners;
    private InstalledMod battleRattle = null;

    public static BattleRattleCompatibility Instance {
      get {
        var initializer = GameObject.Find("BattleRattleInitializer");
        return initializer.GetComponent<BattleRattleCompatibility>();
      }
    }
      
    public void Start() {
      this.enabled = false;

      // As of Alpha 9e, InstalledModLister doesn't return a full listing.
      var allModDirs = new DirectoryInfo(GenFilePaths.ModsFolderPath).GetDirectories();
      var allMods = allModDirs.Select(d => new InstalledMod(d));
      var activeMods = allMods.Where(m => ModsConfig.IsActive(m.FolderName)).ToArray();

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
        Log.Error("Multiple active installations of Battle Rattle found; this won't end well.");
      } 

      // In theory a Class attribute on the ModMetaData element would work 
      // here, but when BattleRattle is inactive, RimWorld won't load the DLL 
      // and so will throw an exception when the user opens the mod listing 
      // dialogue.

      var metaData = XmlLoader.ItemFromXmlFile<BattleRattleMetaData>(
        Path.Combine(battleRattle.dir.ToString(), Path.Combine("About", "About.xml")), false
      );

      Log.Message(
        metaData.name + " " + metaData.version 
        + " checking for compatible and incompatible mods:"
      );

      foreach (var mod in activeMods) {
        if (mod.Name == "Better Than Sentry Guns") {
          RegisterCompatibility<BetterThanSentryGuns>(mod);

        } else if (mod.Name == "[T] ExpandedCloth") {
          Log.Warning(" - Compatibility for Expanded Cloth not yet implemented.");

        } else if (Regex.IsMatch(mod.Name, "Extended Woodworking")) {
          RegisterCompatibility<ExpandedWoodworking>(mod);

        } else if (Regex.IsMatch(mod.Name, "Glassworks")) {
          RegisterCompatibility<Glassworks>(mod);

        } else if (Regex.IsMatch(mod.Name, "NonLethals")) {
          RegisterCompatibility<NonLethals>(mod);

        } else if (mod.Name == "Project Armory") {
          Log.Warning(" - Compatibility for Project Armory not yet implemented.");

        } else if (Regex.IsMatch(mod.Name, "Right Tool For The Job")) {
          RegisterCompatibility<RightToolForTheJob>(mod);
        
        } else if (mod.Name == "Rimsearch") {
          RegisterCompatibility<Rimsearch>(mod);
         
        } else if (mod.Name == "Thingamajigs") {
          RegisterCompatibility<Thingamjigs>(mod);
                  
        } else if (mod.Name == "Winter Is Here") {
          RegisterCompatibility<WinterIsHere>(mod);
        }
      }

      Log.Message(" * Compatibility checking complete.");
    }

    private void RegisterCompatibility<T>(InstalledMod mod) {
      if (this.listeners == null) {
        this.listeners = new List<ICompatibility>();
      }

      try {
        var compatibility = (ICompatibility) Activator.CreateInstance<T>();
        compatibility.Inject(mod, this.battleRattle);
        this.listeners.Add(compatibility);

        Log.Message(" + Injected compatibility for " + mod.Name + ".");

      } catch (Exception ex) {
        Log.Error(
          " - Failed injecting compatibility for " + mod.Name + ": " + ex
        );
      }
    }

    public void ResearchDone(string name) {
      foreach(var l in this.listeners) {
        l.ResearchDone(name);
      }
    }

  }
}
