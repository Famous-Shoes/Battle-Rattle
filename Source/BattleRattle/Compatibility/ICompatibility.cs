using System;

namespace BattleRattle.Compatibility {
  public interface ICompatibility {

    void Inject();
    void ResearchDone(string researchDefName);

  }
}
