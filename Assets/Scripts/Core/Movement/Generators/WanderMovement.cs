using Common;
using UnityEngine;
using UnityEngine.AI;

namespace Core
{
    internal sealed class WanderMovement : MovementGenerator
    {
        private TimeTracker nextMoveTime = new TimeTracker();
        private readonly NavMeshPath wanderNavMeshPath = new NavMeshPath();

        public override MovementType Type => MovementType.Wander;
        private static System.Random RandomGen = new System.Random();

        public override void Begin(Unit unit)
        {
            nextMoveTime.Reset(10);

            unit.AI.NavMeshAgentEnabled = true;
            unit.AI.UpdatePosition = false;
            unit.AI.Speed = unit.RunSpeed;
            unit.AI.AngularSpeed = MovementUtils.MoveRotationSpeed;
        }

        public override void Finish(Unit unit)
        {
            unit.AI.NavMeshAgentEnabled = false;
            unit.RemoveState(UnitControlState.Wander);
            unit.StopMoving();

            nextMoveTime.Reset(0);
        }

        public override void Reset(Unit unit)
        {
            nextMoveTime.Reset(0);
            unit.StopMoving();
        }

        public override bool Update(Unit unit, int deltaTime)
        {
            bool cantMove = unit.HasAnyState(UnitControlState.Root | UnitControlState.Stunned | UnitControlState.Distracted);
            unit.AI.UpdateRotation = !cantMove;

            if (cantMove)
            {
                unit.AI.NextPosition = unit.Position;
                unit.SetMovementFlag(MovementFlags.MaskMoving, false);
            }
            else if (unit.AI.HasPath)
            {
                Vector3 localDirection = unit.transform.TransformDirection(unit.AI.NextPosition - unit.Position);

                unit.SetMovementFlag(MovementFlags.Forward, localDirection.z > MovementUtils.DirectionalMovementThreshold);
                unit.SetMovementFlag(MovementFlags.Backward, localDirection.z < -MovementUtils.DirectionalMovementThreshold);
                unit.SetMovementFlag(MovementFlags.StrafeLeft, localDirection.x < -MovementUtils.DirectionalMovementThreshold);
                unit.SetMovementFlag(MovementFlags.StrafeRight, localDirection.x > MovementUtils.DirectionalMovementThreshold);

                unit.Position = unit.AI.NextPosition;
            }

            if (cantMove)
                return true;

            if (nextMoveTime.Passed)
                nextMoveTime.Reset(RandomUtils.Next(1000, 3000));
                //nextMoveTime.Reset(1000);
            else
            {
                nextMoveTime.Update(deltaTime);
                if (nextMoveTime.Passed)
                {
                    unit.AddState(UnitControlState.Wander);

                    Vector2 randomCircle = Random.insideUnitCircle * 6;
                    Vector3 randomPosition = unit.Position + new Vector3(randomCircle.x, 0, randomCircle.y);
                    // Use wander nodes
                    //if (!NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, MovementUtils.MaxNavMeshSampleRange, MovementUtils.WalkableAreaMask))
                    //    return TryAgainSoon();
                    //randomPosition = hit.position;

                    int RandomNumb = RandomGen.Next(100);
                    if (RandomNumb < 20)
                    {
                        randomPosition.x = -28.947F;
                        randomPosition.y = 0.002F;
                        randomPosition.z = -3.875F;
                    }
                    else if (RandomNumb < 40)
                    {
                        randomPosition.x = -19.214F;
                        randomPosition.y = -0.599F;
                        randomPosition.z = 4.009F;
                    }
                    else if (RandomNumb < 60)
                    {
                        randomPosition.x = 12.316F;
                        randomPosition.y = -0.188F;
                        randomPosition.z = -7.486F;
                    }
                    else if (RandomNumb < 60)
                    {
                        randomPosition.x = -27.3932F;
                        randomPosition.y = -1.137F;
                        randomPosition.z = -9.289F;
                    }
                    else
                    {
                        randomPosition.x = 0.0782F;
                        randomPosition.y = -0.438F;
                        randomPosition.z = -9.5125F;
                    }

                    if (!NavMesh.CalculatePath(unit.Position, randomPosition, MovementUtils.WalkableAreaMask, wanderNavMeshPath))
                        return TryAgainSoon();

                    if (!unit.AI.SetPath(wanderNavMeshPath))
                        return TryAgainSoon();

                    if (unit.AI.RemainingDistance > MovementUtils.MaxConfusedPath)
                        return TryAgainSoon();
                }
            }

            return true;

            bool TryAgainSoon()
            {
                nextMoveTime.Reset(100);
                return true;
            }
        }
    }
}
