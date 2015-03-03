using Verse;


namespace BattleRattle.WeaponCarriers {
  public interface IWeaponCarrier {
    
    bool CanStoreThing(Thing thing);

    bool StorePrimary();
    bool EquipPrimary();
    bool RemoveStored();
  }
}
