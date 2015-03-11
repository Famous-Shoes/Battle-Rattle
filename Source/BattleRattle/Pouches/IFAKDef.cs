using Verse;


namespace BattleRattle.Pouches {
  public class IFAKDef : ThingDef {

    public int ticksForPacking;
    public int packRadius;

    public static IFAKDef Instance {
      get {
        return (IFAKDef) ThingDef.Named(
          "BattleRattle_Pouches_IFAK"
        );
      }
    }

  }
}
