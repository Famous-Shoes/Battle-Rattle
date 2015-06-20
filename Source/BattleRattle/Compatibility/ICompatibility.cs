using Verse;

namespace BattleRattle.Compatibility {
  public interface ICompatibility {

    void Inject(InstalledMod mod, InstalledMod battleRattle);
    void ResearchDone(string researchDefName);

  }
}
