using System;
using Verse;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BattleRattle  {
  public class BattleRattleInitializer: ITab  {

    private GameObject instance;

    public BattleRattleInitializer() {
      this.instance = new GameObject("BattleRattleInitializer");
      this.instance.AddComponent<BattleRattleCompatibility>();
      UnityEngine.Object.DontDestroyOnLoad(this.instance);
    }

    protected override void FillTab () {}
  }
}

