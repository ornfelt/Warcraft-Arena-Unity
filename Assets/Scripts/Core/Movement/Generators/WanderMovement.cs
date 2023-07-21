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
            new Vector3(-19.13F, -0.60F, 17.18F), new Vector3(-17.87F, -1.20F, -18.05F), 
            new Vector3(13.87F, -0.44F, -18.09F), new Vector3(12.84F, -0.63F, 18.75F), 
            new Vector3(0.97F, -0.44F, -24.34F), new Vector3(0.39F, -0.43F, 26.41F)
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

                    Debug.Log("HasReachedNode: " + unit.CurrWanderNode);
                    int newWanderNode;
                    if (unit.CurrWanderNode == 100)
                        newWanderNode = GetClosestWanderNode(unit.Position);
                    else
                        newWanderNode = GetNextNode();

                    randomPosition = WanderNodes[newWanderNode];
                    unit.CurrWanderNode = newWanderNode;
                    Debug.Log("newNode: " + unit.CurrWanderNode);

                    if (!NavMesh.CalculatePath(unit.Position, randomPosition, MovementUtils.WalkableAreaMask, wanderNavMeshPath))
                        return TryAgainSoon();

                    if (!unit.AI.SetPath(wanderNavMeshPath))
                        return TryAgainSoon();
                }
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
                return (diffX + diffY + diffZ) < 0.5;
            }

            int GetNextNode()
            {
                if (unit.CurrWanderNode == 5)
                    return 0;
                else
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
