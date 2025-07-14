using EmptyKeys.UserInterface.Generated.PlayerTradeView_Bindings;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using VRageRender;

namespace HardStart
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_VirtualMass), false, new[] { "TestBlockSmall" })]
    internal class TestBlock : MyGameLogicComponent
    {
        private IMyFunctionalBlock block;
        private MyCubeGrid grid;

        private MyParticleEffect effect;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            block = Entity as IMyFunctionalBlock;
            grid = block.CubeGrid as MyCubeGrid;

            //NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            //NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            //NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            var ent = (MyEntity)block;
            ent.OnModelRefresh += BlockModelChanged;
            BlockModelChanged(ent);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }   
        public override void UpdateAfterSimulation()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
        }

        public override void UpdateAfterSimulation10()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
        }

        public override void UpdateAfterSimulation100()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            Log.Msg($"Tick {block.CubeGrid.DisplayName}");
            if (effect == null && block.IsFunctional)
            {
                Log.Msg("Creating effect");
                MatrixD localMatrix = block.LocalMatrix;
                effect = SpawnParticle("ExhaustSmokeSmall", ref localMatrix);
                if (effect == null)
                    Log.Msg("Create Failed");
            }
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();

        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            Log.Msg($"OnRemovedFromScene {block.CubeGrid.DisplayName}");

        }

        public override void MarkForClose()
        {
            base.MarkForClose();
            Log.Msg($"MarkForClose {block.CubeGrid.DisplayName}");
            RemoveParticle();
        }

        void BlockModelChanged(MyEntity ent)
        {
            RemoveParticle();
        }

        private MyParticleEffect SpawnParticle(string subtypeId, ref MatrixD  localMatrix)
        {
            MyParticleEffect effect;
            Vector3D worldPos = block.GetPosition();
            uint parentId = block.Render.GetRenderObjectID();
            if (!MyParticlesManager.TryCreateParticleEffect(subtypeId, ref localMatrix, ref worldPos, parentId, out effect))
                return null;

            return effect;
        }

        private void RemoveParticle()
        {
            if (effect != null)
            {
                MyParticlesManager.RemoveParticleEffect(effect);
                effect = null;
            }
        }
    }
}