using System;

using Verse;

namespace BattleRattle.Rucks {
  public class RuckFilterWorker: SpecialThingFilterWorker {

    public IRuck Ruck {get; set;}

    public override bool Matches(Thing thing) {
      return Ruck.CanFit(thing) <= 0;
    }

  }
}
