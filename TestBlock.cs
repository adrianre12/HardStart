using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using VRageRender;

namespace SEtest
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_VirtualMass), false, new[] { "TestBlockSmall" })]
    internal class TestBlock : MyGameLogicComponent
    {
        private IMyFunctionalBlock block;
        private MyCubeGrid grid;
        private float rotationSpeed = 0.05f;
        private float targetSpeed = 1f ;
        private float deaccelaration = 0.4f;
        private MyPlanet closestPlanet;
        private double height;
        private bool active;
        private double deployHeight = 15;
        private Vector3 up;
        private Quaternion to;
        private Quaternion from;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            block = Entity as IMyFunctionalBlock;
            grid = block.CubeGrid as MyCubeGrid;

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;

        }
        public override void UpdateAfterSimulation()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            if (active && grid.Physics.Speed > 0.1)
            {
                up = -grid.Physics.LinearVelocity;
                from = Quaternion.CreateFromForwardUp(grid.WorldMatrix.Forward, grid.WorldMatrix.Up);
                to = Quaternion.CreateFromForwardUp(Vector3D.Cross(Vector3D.Cross(up, grid.WorldMatrix.Forward), up), up);

                var newMatrix = MatrixD.CreateFromQuaternion(Quaternion.Lerp(from, to, rotationSpeed));
                newMatrix.Translation = grid.WorldMatrix.Translation;

                grid.WorldMatrix = newMatrix;
                var speed = grid.Physics.Speed;
                var multiplier = targetSpeed / speed;
                if (speed - deaccelaration > targetSpeed)
                    multiplier = (speed - deaccelaration) / speed;

                grid.Physics.LinearVelocity = grid.Physics.LinearVelocity * multiplier;
            }
        }

        public override void UpdateAfterSimulation10()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            if (height < 1) // landed or no planet
                return;
            if (closestPlanet != null )
                height = closestPlanet.GetHeightFromSurface(grid.WorldMatrix.Translation);
            active = (height < deployHeight && height > 1);
            Log.Msg($"height={height} active={active} speed={grid.Physics.Speed}");
        }

        public override void UpdateAfterSimulation100()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            Log.Msg($"Tick {block.CubeGrid.DisplayName}");
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            Log.Msg($"OnAddedToScene {block.CubeGrid.DisplayName}");
            closestPlanet = MyGamePruningStructure.GetClosestPlanet(grid.WorldMatrix.Translation);
            if (closestPlanet != null)
                height = closestPlanet.GetHeightFromSurface(grid.WorldMatrix.Translation);
            Log.Msg($"Start height={height}");
        }

        public override void OnRemovedFromScene()
        {
            Log.Msg($"OnRemovedFromScene {block.CubeGrid.DisplayName}");

        }

        public override void MarkForClose()
        {
            base.MarkForClose();
            Log.Msg($"MarkForClose {block.CubeGrid.DisplayName}");

        }
    }
}