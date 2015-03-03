using Verse;

namespace BattleRattle.Apparel {
  public interface IApparel {

    bool IsWorn {get;}
    void OnWorn();
    void OnDropped();

  }
}
