using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace HardStart
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false, new[] { "EmergencySmallGenerator" })]
    internal class AtomicBattery : MyGameLogicComponent
    {
        private IMyCubeBlock block;
        private List<VRage.Game.ModAPI.Ingame.MyInventoryItem> inventory = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            //base.Init(objectBuilder);

            Log.Msg("AB Init...");
           // if (!MyAPIGateway.Session.IsServer)
            //    return;

            if (MyAPIGateway.Utilities.IsDedicated)
                return;

            block = Entity as IMyCubeBlock;

            if (block.CubeGrid?.Physics == null)
                return;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            Log.Msg("AB Init done");

        }

        public override void UpdateOnceBeforeFrame()
        {
            Log.Msg("AB UpdateOnceBeforeFrame");
            base.UpdateOnceBeforeFrame();
            //Temp();
            CheckTCellVisability();
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            Log.Msg("UpdateAfterSimulation100");
            Temp();
            CheckTCellVisability();
        }

        private void Temp()
        {
            VRage.Game.ModAPI.Ingame.MyItemType TritiumCellId = VRage.Game.ModAPI.Ingame.MyItemType.MakeComponent("EmergencyTritiumCell");
            inventory.Clear();
            block.GetInventory().GetItems(inventory);

            if (inventory == null)
            {
                Log.Msg("inventory null");
                return;
            }
            foreach (var item in inventory)
            {
                if(item.Type == TritiumCellId)
                Log.Msg($"Found TritiumCell amount={item.Amount}");
            }
        }

        private void CheckTCellVisability()
        {
            Log.Msg("CheckTCellVisability");
            VRage.Game.ModAPI.Ingame.MyItemType tcellType = VRage.Game.ModAPI.Ingame.MyItemType.MakeComponent("EmergencyTritiumCell");
            if (block.GetInventory().ContainItems((MyFixedPoint)0.001, tcellType))
            {
                Log.Msg("found Tritium");
                UpdateTCellVisability(true);
            }
            else
            {
                Log.Msg("no Tritium");
                UpdateTCellVisability(false);
            }
        }

        private void UpdateTCellVisability(bool visible)
        {
            Log.Msg("UpdateTCellVisability");
            try
            {
                MyEntitySubpart subpart;
                if (Entity.TryGetSubpart("TritiumCell", out subpart)) // subpart does not exist when block is in build stage
                {
                    subpart.Render.Visible = visible;
                }
            }
            catch (Exception e)
            {
                Log.Msg(e.ToString());
            }
    }
}
    }

