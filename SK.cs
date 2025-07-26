using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace HardStart
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_SurvivalKit), false, new[] { "EmergencySurvivalKit" })]
    internal class SurvivalKit : MyGameLogicComponent
    {
        private IMyFunctionalBlock block;
        private MyCubeGrid grid;
        private List<IMySlimBlock> blocks = new List<IMySlimBlock>();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            block = Entity as IMyFunctionalBlock;
            grid = block.CubeGrid as MyCubeGrid;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            CheckForCockpit();
            grid.OnBlockAdded += Grid_OnBlockChanged;
            grid.OnBlockRemoved += Grid_OnBlockChanged;
        }
        private void Grid_OnBlockChanged(IMySlimBlock obj)
        {
            CheckForCockpit();
        }

        private void CheckForCockpit() {
            if (!block.IsFunctional)
                return;
            blocks.Clear();
            var cockpit = grid.GetFirstBlockOfType<MyCockpit>();
            if (cockpit != null)
            {
                //Log.Msg("Found cockpit");
                block.CubeGrid.IsStatic = false;
            }
            else
            {
                //Log.Msg("No cockpit");
                block.CubeGrid.IsStatic = true;
            }
        }
    }
}
