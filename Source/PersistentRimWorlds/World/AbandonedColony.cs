using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PersistentWorlds.World
{
    [StaticConstructorOnStartup]
    public sealed class AbandonedColony : MapParent
    {
        #region Fields
        private static readonly Texture2D BaseResettleCommand =
            ContentFinder<Texture2D>.Get("UI/Commands/BaseResettle");
        #endregion
        
        #region Methods
        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            foreach (var gizmo in base.GetCaravanGizmos(caravan))
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = "FilUnderscore.PersistentRimWorlds.Command.BaseResettle".Translate(),
                defaultDesc = "FilUnderscore.PersistentRimWorlds.Command.BaseResettle.Desc".Translate(),
                icon = BaseResettleCommand,
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("Resettle, Roll the Dice and see whether it will be overtaken by enemy base already.", "Dismiss"));
                
                    /*
                    Settlement resettledHome = SettleUtility.AddNewHome(caravan.Tile, caravan.Faction);
                    
                    var map = GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, Find.World.info.initialMapSize,
                        null);
                    
                    // Roll dice.
                    
                    MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out var spot, out var second);
                    CaravanEnterMapUtility.Enter(caravan, map, (Pawn p) => CellFinder.RandomSpawnCellForPawnNear(spot, map, 4), CaravanDropInventoryMode.DoNotDrop, false);
                    CameraJumper.TryJump(caravan.PawnsListForReading[0]);
                    */

                    var map = Current.Game.FindMap(caravan.Tile);
                    var daysAway = 200 - 10;
                
                    foreach (var thing in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                    {
                        if (Rand.Chance(0.005f * daysAway))
                        {
                            thing.TakeDamage(new DamageInfo(DamageDefOf.Deterioration,
                                Rand.Range(0, thing.MaxHitPoints * 1.5F)));
                        }
                    }
                
                    Find.WorldObjects.Remove(Find.WorldObjects.WorldObjectAt(caravan.Tile, WorldObjectDefOf.AbandonedSettlement));
                    SettleInExistingMapUtility.Settle(map);
                    CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge);
                }
            };
        }
        #endregion
    }
}