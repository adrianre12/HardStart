using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
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
        private IMyTerminalBlock block;
        private float tritiumAmount;
        private MyInventory inventory;
        private MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_Component), "EmergencyTritiumCell");


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Log.Msg("AtomicBattery Init...");
            // if (!MyAPIGateway.Session.IsServer)
            //    return;

            if (MyAPIGateway.Utilities.IsDedicated) // only run on client
                 return;
            //Log.Msg($"IsServer={MyAPIGateway.Session.IsServer} IsDedicated={MyAPIGateway.Utilities.IsDedicated}");

            block = Entity as IMyTerminalBlock;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

        }

        void AppendingCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            try 
            {
                sb.Append($"Tritium Cell: {(tritiumAmount*100):0.00}%\n");
            }
            catch (Exception e)
            {
                Log.Msg(e.Message);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (block.CubeGrid?.Physics == null)
                return;

            inventory = block.GetInventory() as MyInventory;

            CheckTCellVisability();

            block.AppendingCustomInfo += AppendingCustomInfo;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            CheckTCellVisability();

            try { 
                if (MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.ControlPanel)
                {
                    block.RefreshCustomInfo();
                    block.SetDetailedInfoDirty();
                }
            }
            catch (Exception e)
            {
                Log.Msg(e.Message);
            }
        }

        private void CheckTCellVisability()
        {
            tritiumAmount = (float)inventory.GetItemAmount(id);

            try
            {
                MyEntitySubpart subpart;
                if (Entity.TryGetSubpart("TritiumCell", out subpart)) // subpart does not exist when block is in build stage
                {
                    subpart.Render.Visible = tritiumAmount > 0.001;
                }
            }
            catch (Exception e)
            {
                Log.Msg(e.ToString());
            }
        }
    }
}

