using System;

using Verse;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

namespace BattleRattle.Utility {
  public static class Buttons {

    private static readonly IDictionary<string, Texture2D> TEXTURES;

    static Buttons() {
      TEXTURES = new Dictionary<string, Texture2D>();
    }

    public static Texture2D Icon(Thing thing, string name) {
      Texture2D texture;
      TEXTURES.TryGetValue(thing.def.defName + name, out texture);

      if (texture == null) {
        texture = ContentFinder<Texture2D>.Get(
          thing.def.defName.Replace("_", "/") + "/Button_" + name, true
        );
      }

      return texture;
    }

  }
}
