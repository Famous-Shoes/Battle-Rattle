using System;
using Verse;
using UnityEngine;

namespace BattleRattle.Rucks {
  public class PackRuck_SettingsTab: ITab {

    private Vector2 scrollPosition;

    public PackRuck_SettingsTab() {
      this.size = new Vector2(300f, 480f);
      this.labelKey = "BattleRattle.Rucks.PackRuck.SettingsTab";
    }

    protected override void FillTab() {
      var ruck = (IRuck) base.SelThing;

      GUI.Label(new Rect(10f, 20f, 150f, 20f), "Pack things within: " + ruck.PackRadius);
      ruck.PackRadius = (int) GUI.HorizontalSlider(new Rect(10f, 50f, 150f, 20f), (float) ruck.PackRadius, 1f, 100f);

      if (Widgets.TextButton(new Rect(190f, 30f, 100f, 50f), "Clear All")) {
        ruck.PackableCurrent.SetDisallowAll();
      }

      ThingFilterUI.DoThingFilterConfigWindow(new Rect(10f, 110f, this.size.x - 20f, this.size.y - 120f), ref this.scrollPosition, ruck.PackableCurrent, ruck.PackableAll);
      GUI.EndGroup();
    }
  }
}

