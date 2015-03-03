using Verse;

namespace BattleRattle.Rucks {
  public class MatchAllFilterWorker: SpecialThingFilterWorker {

    public override bool Matches(Thing thing) {
      return true;
    }

  }
}
