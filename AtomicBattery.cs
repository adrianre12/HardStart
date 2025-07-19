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
            Log.Msg("AtomicBattery Init...");
            // if (!MyAPIGateway.Session.IsServer)
            //    return;

            if (MyAPIGateway.Utilities.IsDedicated) // only run on client
                 return;
            //Log.Msg($"IsServer={MyAPIGateway.Session.IsServer} IsDedicated={MyAPIGateway.Utilities.IsDedicated}");

            block = Entity as IMyCubeBlock;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

        }

        public override void UpdateOnceBeforeFrame()
        {
            //Log.Msg("AB UpdateOnceBeforeFrame");
            base.UpdateOnceBeforeFrame();
            if (block.CubeGrid?.Physics == null)
                return;

            CheckTCellVisability();
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            //Log.Msg("UpdateAfterSimulation100");
            CheckTCellVisability();
        }

        private void CheckTCellVisability()
        {
            //Log.Msg("CheckTCellVisability");
            VRage.Game.ModAPI.Ingame.MyItemType tcellType = VRage.Game.ModAPI.Ingame.MyItemType.MakeComponent("EmergencyTritiumCell");
            if (block.GetInventory().ContainItems((MyFixedPoint)0.001, tcellType))
            {
                //Log.Msg("found Tritium");
                UpdateTCellVisability(true);
            }
            else
            {
                //Log.Msg("no Tritium");
                UpdateTCellVisability(false);
            }
        }

        private void UpdateTCellVisability(bool visible)
        {
            //Log.Msg("UpdateTCellVisability");
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

