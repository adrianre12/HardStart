using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using System;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace HardStart
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false, new[] { "SmallEjectorSeatRocket" })]
    internal class RocketBlock : MyGameLogicComponent
    {
        const float startHeight = 200;
        const float stopHeight = 2;
        const float targetSpeedHigh = 5f;
        const float targetSpeedLow = 1.0f;
        const float speedChangeHeight = 5;
        const float rotationSpeed = 0.01f;

        private IMyThrust block;
        private MyCubeGrid grid;
        private float targetSpeed = targetSpeedHigh;
        private MyPlanet closestPlanet;
        private double height;
        private bool active;
        private Quaternion to;
        private Quaternion from;
        private bool landed = false;
        private float decreaseStep;
        private float targetSpeedSlope;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            block = Entity as IMyThrust;
            grid = block.CubeGrid as MyCubeGrid;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;

            Log.Msg($"UpdateOnceBeforeFrame {block.CubeGrid.DisplayName}");
            closestPlanet = MyGamePruningStructure.GetClosestPlanet(grid.WorldMatrix.Translation);
            if (closestPlanet != null)
                height = closestPlanet.GetHeightFromSurface(grid.WorldMatrix.Translation);
            block.Enabled = true;
            //Log.Msg($"Start height={height} override={block.ThrustOverridePercentage} currentThrust={block.CurrentThrust}  dampeners={grid.DampenersEnabled}");

            var gravityUp = Vector3.Normalize(-grid.NaturalGravity);

            Log.Msg($"Gravity={gravityUp.Length()} vector={gravityUp}");

            to = Quaternion.CreateFromForwardUp(grid.WorldMatrix.Forward, gravityUp); // use gravity to allow for testing by pasting
            to.Normalize();

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            if (active && grid.Physics.Speed > 0.1)
            {  
                from = Quaternion.CreateFromForwardUp(grid.WorldMatrix.Forward, grid.WorldMatrix.Up);
                from.Normalize();

                var newMatrix = MatrixD.CreateFromQuaternion(Quaternion.Slerp(from, to, rotationSpeed));
                newMatrix.Translation = grid.WorldMatrix.Translation;

                grid.WorldMatrix = newMatrix;

                var speed = grid.Physics.Speed;
                var multiplier = targetSpeed / speed;
                //Log.Msg($"speed={speed} multiplier={multiplier}");

                if (speed - decreaseStep > targetSpeed)
                    multiplier = (speed - decreaseStep) / speed;
                grid.Physics.LinearVelocity = grid.Physics.LinearVelocity * multiplier;
                grid.Physics.AngularVelocity = Vector3.Zero;
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            if (landed) // landed or no planet
                return;
            
            if (closestPlanet != null )
                height = closestPlanet.GetHeightFromSurface(grid.WorldMatrix.Translation);
            active = (height < startHeight && height > stopHeight);
            if (active)
            {
                block.ThrustOverridePercentage = 1f;
                if (height < speedChangeHeight)
                {
                    targetSpeed = targetSpeedLow;
                } else
                {
                    targetSpeed = (float)((height- speedChangeHeight) * targetSpeedSlope);
                    targetSpeed = Math.Max(targetSpeed, targetSpeedLow);
                }
            }
            else
            {
                block.ThrustOverridePercentage = 0;
                decreaseStep = grid.Physics.Speed / 150;
                targetSpeedSlope =  (grid.Physics.Speed- targetSpeedLow)/(startHeight-speedChangeHeight);
                Log.Msg($"targetSpeedSlope={targetSpeedSlope}");
            }

            if (height < stopHeight || grid.Physics.Speed < 0.05)
            {
                Log.Msg($"Landed height={height} speed={grid.Physics.Speed}");
                landed = true;
                block.Enabled = false;
            }
            Log.Msg($"height={height} active={active} speed={grid.Physics.Speed} override={block.ThrustOverridePercentage} currentThrust={block.CurrentThrust} dampners={grid.DampenersEnabled}");
            
        }
    }
}