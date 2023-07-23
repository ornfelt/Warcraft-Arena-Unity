using Common;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Drawing.Text;

namespace Core
{
    internal sealed class WanderMovement : MovementGenerator
    {
        private TimeTracker nextMoveTime = new TimeTracker();
        private readonly NavMeshPath wanderNavMeshPath = new NavMeshPath();

        public override MovementType Type => MovementType.Wander;
        private static System.Random RandomGen = new System.Random();

        private static List<Vector3> WanderNodes = new List<Vector3> {
            new Vector3(-0.23F, -0.58F, 31.69F), new Vector3(5.11F, -0.44F, 27.19F),
            new Vector3(11.30F, -0.68F, 21.9F), new Vector3(16.53F, -0.92F, 17.36F),
            new Vector3(19.79F, -0.16F, 12.38F), new Vector3(21.53F, 1.15F, 6.72F),
            new Vector3(22.62F, 1.97F, -1.65F), new Vector3(19.97F, 1.06F, -7.49F),
            new Vector3(16.50F, -0.08F, -12.72F), new Vector3(12.83F, -0.44F, -18.19F),
            new Vector3(7.70F, -0.44F, -23.46F), new Vector3(1.10F, -0.43F, -26.94F),
            new Vector3(-5.20F, -0.43F, -26.87F), new Vector3(-10.73F, -0.64F, -23.41F),
            new Vector3(-15.93F, -0.94F, -17.78F), new Vector3(-20.46F, -1.35F, -11.46F),
            new Vector3(-24.07F, -1.07F, -3.50F), new Vector3(-25.47F, -1.08F, 3.55F),
            new Vector3(-24.31F, -1.04F, 11.24F), new Vector3(-20.46F, -0.61F, 18.46F),
            new Vector3(-14.83F, -0.49F, 22.16F), new Vector3(-7.81F, -0.43F, 25.83F)
        };
        private Vector3 LastVisitedNode;

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
            else
            {
                nextMoveTime.Update(deltaTime);
                //if (nextMoveTime.Passed)
                if (HasReachedNode())
                {
                    unit.AddState(UnitControlState.Wander);

                    Vector2 randomCircle = Random.insideUnitCircle * 6;
                    Vector3 randomPosition = unit.Position + new Vector3(randomCircle.x, 0, randomCircle.y);

                    int newWanderNode;
                    if (unit.CurrWanderNode == 100)
                        newWanderNode = GetClosestWanderNode(unit.Position);
                    else
                        newWanderNode = GetNextNode();

                    randomPosition = WanderNodes[newWanderNode];
                    unit.CurrWanderNode = newWanderNode;

                    if (!NavMesh.CalculatePath(unit.Position, randomPosition, MovementUtils.WalkableAreaMask, wanderNavMeshPath))
                    {
                        unit.AI.SetDestination(randomPosition);
                        return TryAgainSoon();
                    }

                    if (!unit.AI.SetPath(wanderNavMeshPath))
                        return TryAgainSoon();
                }
                //else return TryAgainSoon();
            }

            return true;

            bool TryAgainSoon()
            {
                nextMoveTime.Reset(100);
                return true;
            }

            bool HasReachedNode()
            {
                if (unit.CurrWanderNode == 100)
                    return true;
                float diffX = Mathf.Abs(unit.Position.x - WanderNodes[unit.CurrWanderNode].x);
                float diffY = Mathf.Abs(unit.Position.y - WanderNodes[unit.CurrWanderNode].y);
                float diffZ = Mathf.Abs(unit.Position.z - WanderNodes[unit.CurrWanderNode].z);

                return (diffX + diffY + diffZ) < 7.0F;
            }

            int GetNextNode()
            {
                int RandomNumb = RandomGen.Next(100);
                if (unit.CurrWanderNode == 20)
                    return 0;
                else
                    //return RandomNumb < 50 ? unit.CurrWanderNode + 1 : unit.CurrWanderNode -1;
                    return unit.CurrWanderNode + 1;
            }

            int GetClosestWanderNode(Vector3 myPos)
            {
                int closestNode = 0;
                float diffX, diffY, diffZ;
                float closestDiff = 100.0F;
                int nodeIdx = 0;

                foreach (var node in WanderNodes)
                {
                    diffX = Mathf.Abs(myPos.x - node.x);
                    diffY = Mathf.Abs(myPos.y - node.y);
                    diffZ = Mathf.Abs(myPos.z - node.z);
                    float currDiff = diffX + diffY + diffZ;
                    if (currDiff > 1.0F && currDiff < closestDiff)
                    {
                        closestNode = nodeIdx;
                        closestDiff = currDiff;
                    }
                    ++nodeIdx;
                }

                return closestNode;
            }
        }
    }
}
