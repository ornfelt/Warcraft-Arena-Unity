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

        private static List<Vector3> WanderNodes = new List<Vector3>
        {
            new Vector3(-0.10F, -0.73F, 34.75F), new Vector3(25.08F, 0.36F, 12.82F),
            new Vector3(15.87F, -0.46F, -14.57F), new Vector3(-0.05F, -0.48F, -34.11F),
            new Vector3(-29.42F, -1.21F, -7.67F), new Vector3(-20.62F, -0.60F, 20.20F)
        };

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
            if (!cantMove)
                if (unit.ClosestPlayerTarget(unit.Position, 16.0F) || !unit.IsAlive)
                    cantMove = true;
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
                //nextMoveTime.Reset(RandomUtils.Next(1000, 3000));
                nextMoveTime.Reset(RandomUtils.Next(2500, 4000));
            else
            {
                nextMoveTime.Update(deltaTime);
                //if (nextMoveTime.Passed || unit.CurrWanderNode == 100)
                if (HasReachedNode())
                {
                    unit.AddState(UnitControlState.Wander);

                    //Vector2 randomCircle = Random.insideUnitCircle * 6;
                    //Vector3 randomPosition = unit.Position + new Vector3(randomCircle.x, 0, randomCircle.y);
                    Vector3 randomPosition;

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
                else return TryAgainSoon();
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

                return (Vector3.Distance(unit.Position, WanderNodes[unit.CurrWanderNode])) < 5.0F;
            }

            int GetNextNode()
            {
                int RandomNumb = RandomGen.Next(100);
                if (unit.CurrWanderNode == WanderNodes.Count - 1)
                    return 0;
                else
                    //return RandomNumb < 95 ? unit.CurrWanderNode + 1 : unit.CurrWanderNode -1;
                    return unit.CurrWanderNode + 1;
            }

            int GetClosestWanderNode(Vector3 myPos)
            {
                int closestNode = 0;
                float closestDiff = 100.0F;
                int nodeIdx = 0;

                foreach (var node in WanderNodes)
                {
                    float currDiff = Vector3.Distance(myPos, node);
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
