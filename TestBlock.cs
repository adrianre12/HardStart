using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace SEtest
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_VirtualMass), false, new[] { "TestBlockSmall" })]
    internal class TestBlock : MyGameLogicComponent
    {
        private IMyFunctionalBlock block;
        private MyCubeGrid grid;
        private int ticker;
        private MatrixD gridPosition;
        private float rotationSpeed = 0.05f;
        private float fallSpeed = 1f ;
        private bool updateRotate = true;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            block = Entity as IMyFunctionalBlock;
            grid = block.CubeGrid as MyCubeGrid;

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;

        }
        public override void UpdateAfterSimulation()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            ticker++;
            //grid.Physics.ClearSpeed();
            if (grid.Physics.Speed > 0.1)
            {
                grid.Physics.LinearVelocity = (grid.Physics.LinearVelocity / grid.Physics.Speed) * fallSpeed;
            }

            /*                var from = Quaternion.CreateFromForwardUp(grid.WorldMatrix.Forward, grid.WorldMatrix.Up);
                            var forward = Vector3D.Cross(Vector3D.Cross(-grid.Physics.LinearVelocity, grid.WorldMatrix.Forward), -grid.Physics.LinearVelocity);
                            var to = Quaternion.CreateFromTwoVectors( forward , -grid.Physics.LinearVelocity);
                            var newUp = Quaternion.Lerp(from, to, rotationSpeed);

                            var newMatrix = MatrixD.CreateFromQuaternion(newUp);*/



            if (updateRotate)
            {
                //updateRotate = false;

                Log.Msg($"forward={grid.WorldMatrix.Forward} up={grid.WorldMatrix.Up}");
                Log.Msg($"grav={Vector3D.Normalize(-grid.Physics.LinearVelocity)}");
                
                var from = Quaternion.CreateFromForwardUp(grid.WorldMatrix.Forward, grid.WorldMatrix.Up);
                var forward = Vector3D.Cross(Vector3D.Cross(-grid.Physics.LinearVelocity, grid.WorldMatrix.Forward), -grid.Physics.LinearVelocity);
                var to = Quaternion.CreateFromForwardUp(forward, -grid.Physics.LinearVelocity);
                var newUp = Quaternion.Lerp(from, to, rotationSpeed);

                var newMatrix = MatrixD.CreateFromQuaternion(newUp);
                newMatrix.Translation = grid.WorldMatrix.Translation;

                //var newMatrix = MatrixD.CreateWorld(gridPosition.Translation, gridPosition.Forward,  Vector3D.Normalize(-grid.Physics.LinearVelocity));

                grid.WorldMatrix = newMatrix;
                Log.Msg($"forward={grid.WorldMatrix.Forward} up={grid.WorldMatrix.Up}");
            }

        }
        /*        public override void UpdateAfterSimulation10()
                {
                    if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                        return;
                    //ticker++;
                    if (gridPosition == null)
                        return ;

                    //grid.PositionComp.SetPosition(gridPosition);
                    grid.Physics.ClearSpeed();

                }*/
        public override void UpdateAfterSimulation100()
        {
            if (block?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            Log.Msg($"Tick {block.CubeGrid.DisplayName} ticker={ticker}");
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            Log.Msg($"OnAddedToScene {block.CubeGrid.DisplayName}");
            gridPosition = grid.WorldMatrix;
            //grid.ConvertToStatic();


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