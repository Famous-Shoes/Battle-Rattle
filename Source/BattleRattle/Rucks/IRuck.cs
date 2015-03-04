using Verse;

namespace BattleRattle.Rucks {
  public interface IRuck {

    int CanFit(Thing thing);

    bool CheckPackable(Thing thing);

    bool IsPackable {get;}

    int PackRadius {get; set;}

    void Pack(Pawn packer, Thing packing);

    IntVec3 Position {get; set;}

    ThingFilter PackableCurrent {get;}

    ThingFilter PackableAll {get;}

  }
}
