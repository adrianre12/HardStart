using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace SEtest
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false, new[] { "SmallBlockRocket" })]
    internal class RocketBlock : MyGameLogicComponent
    {
        private IMyThrust block;
        private MyCubeGrid grid;
        private float rotationSpeed = 0.05f;
        private float targetSpeed = 1f ;
        private float deceleration = 0.49f;
        private MyPlanet closestPlanet;
        private double height;
        private bool active;
        private double deployHeight = 15;
        private Vector3 up;
        private Quaternion to;
        private Quaternion from;
        private bool landed = false;
        private Vector3 gravity;

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
            //block.ThrustOverridePercentage = 0.01f;
            Log.Msg($"Start height={height} override={block.ThrustOverridePercentage} currentThrust={block.CurrentThrust}  dampeners={grid.DampenersEnabled}");

            gravity = grid.NaturalGravity;
            deceleration = gravity.Length() / 2;

            Log.Msg($"Gravity={gravity.Length()}");

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
                up = -gravity;// -grid.Physics.LinearVelocity;            
                from = Quaternion.CreateFromForwardUp(grid.WorldMatrix.Forward, grid.WorldMatrix.Up);

                var newMatrix = MatrixD.CreateFromQuaternion( Quaternion.Slerp(from, from * Quaternion.CreateFromTwoVectors(grid.WorldMatrix.Up, up), rotationSpeed));
                newMatrix.Translation = grid.WorldMatrix.Translation;

                grid.WorldMatrix = newMatrix;

                var speed = grid.Physics.Speed;
                var multiplier = targetSpeed / speed;
                if (speed - deceleration > targetSpeed)
                    multiplier = (speed - deceleration) / speed;

                grid.Physics.LinearVelocity = grid.Physics.LinearVelocity * multiplier;
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
            active = (height < deployHeight && height > 1);
            if (active)
                block.ThrustOverridePercentage = 1f;
            else
                block.ThrustOverridePercentage = 0;

            if (height < 1 || grid.Physics.Speed < 0.05)
            {
                Log.Msg($"Landed height={height} speed={grid.Physics.Speed}");
                landed = true;
                block.Enabled = false;
            }
            Log.Msg($"height={height} active={active} speed={grid.Physics.Speed} override={block.ThrustOverridePercentage} currentThrust={block.CurrentThrust} dampners={grid.DampenersEnabled}");
            
        }
    }
}