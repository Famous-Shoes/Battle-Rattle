using System;

using Verse;
using System.Globalization;
using System.Collections.Generic;

namespace BattleRattle.Things {
  public static class Labels {

    private static readonly IDictionary<string, string> LOWER_CASE;
    private static readonly IDictionary<string, string> TITLE_CASE;

    static Labels() {
      LOWER_CASE = new Dictionary<string, string>();
      TITLE_CASE = new Dictionary<string, string>();
    }

    public static string Brief(Thing thing) {
      return thing.def.label;
    }

    public static string Full(Thing thing) {
      return thing.LabelBase;
    }

    public static string ForTitle(string unformatted) {
      string formatted;

      TITLE_CASE.TryGetValue(unformatted, out formatted);
      if (formatted == null) {
        formatted = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(unformatted);
        TITLE_CASE[unformatted] = formatted;
      }

      return formatted;
    }

    public static string ForSentence(string unformatted) {
      string formatted;

      LOWER_CASE.TryGetValue(unformatted, out formatted);
      if (formatted == null) {
        formatted = CultureInfo.CurrentCulture.TextInfo.ToLower(unformatted);
        LOWER_CASE[unformatted] = formatted;
      }

      return formatted;
    }

    public static string ForSentenceBrief(Thing thing) {
      return ForSentence(Brief(thing));
    }

    public static string ForTitleBrief(Thing thing) {
      return ForTitle(Brief(thing));
    }

    public static string ForTitleFull(Thing thing) {
      return ForTitle(Full(thing));
    }

  }
}
