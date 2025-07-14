using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace HardStart
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenGenerator), false, new[] { "EmergencyOxygenGeneratorSmall" })]
    internal class O2Gen : MyGameLogicComponent
    {
        private IMyCubeBlock block;

        public IMyGasGenerator GasGenerator { get; private set; }

        private MyParticleEffect effect;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            block = Entity as IMyCubeBlock;
            GasGenerator = Entity as IMyGasGenerator;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            var ent = (MyEntity)block;
            ent.OnModelRefresh += BlockModelChanged;
            BlockModelChanged(ent);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            ChangeEffectState();
        }

        public override void MarkForClose()
        {
            base.MarkForClose();
            RemoveParticle();
        }

        void BlockModelChanged(MyEntity ent)
        {
            RemoveParticle();
        }


        private void ChangeEffectState()
        {
            bool currentState = effect != null;
            bool targetState = false;
            MatrixD localMatrix = MatrixD.Identity;
            localMatrix.M42 = 0.3;
            
            //Log.Msg($"localMatrix = {localMatrix} ");
            MyResourceSourceComponent sourceComp = GasGenerator.Components.Get<MyResourceSourceComponent>();
            foreach (MyDefinitionId resourceType in sourceComp.ResourceTypes)
            {
                if (sourceComp.CurrentOutputByType(resourceType) > 0f)
                {
                    targetState = true;
                    break;
                }
            }

            if (targetState != currentState)
            {
                if(targetState)
                {
                    effect = SpawnParticle("ExhaustFireSmokeSmall", ref localMatrix);
    }
                else
                {
                    effect.Stop();
                    effect = null;
                }
            }

        }

        private MyParticleEffect SpawnParticle(string subtypeId, ref MatrixD localMatrix)
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
