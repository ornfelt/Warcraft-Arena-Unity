using Common;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    [UsedImplicitly]
    public class SpellCastBehaviour : UnitStateMachineBehaviour
    {
        [SerializeField, UsedImplicitly] private SpellInfo spellInfo;
        [SerializeField, UsedImplicitly] private int castIntervalMin;
        [SerializeField, UsedImplicitly] private int castIntervalMax;
        // HEHE
        //[SerializeField, UsedImplicitly] private BalanceReference balance;
        private static System.Random RandomGen = new System.Random();

        private TimeTracker castTimeTracker = new TimeTracker();

        protected override void OnStart()
        {
            base.OnStart();

            castTimeTracker.Reset(RandomUtils.Next(castIntervalMin, castIntervalMax));
        }

        protected override void OnActiveUpdate(int deltaTime)
        {
            base.OnActiveUpdate(deltaTime);

            castTimeTracker.Update(deltaTime);

            if (!Unit.IsAlive && Unit.HasFlag(UnitFlags.Wander))
            {
                Unit.RemoveFlag(UnitFlags.Wander);
                Unit.RemoveState(UnitControlState.Wander);
                //Unit.Motion.ModifyConfusedMovement(false);
                Unit.Motion.ModifyWanderMovement(false);
            }

            if (castTimeTracker.Passed)
            {
                int RandomNumb = RandomGen.Next(100);
                SpellInfo newSpellInfo = spellInfo;
                bool targetNear = false;
                Vector3 myPos = Unit.Position;
                Vector3 targetPos = Unit.Position;

                // Pick target
                float radius = 15.0F;
                List<Unit> targets = new List<Unit>();
                Unit.Map.SearchAreaTargets(targets, radius, myPos, Unit, SpellTargetChecks.Enemy);

                Unit closestTarget = GetClosestPlayerTarget(myPos, targets);
                if (closestTarget)
                {
                    targetNear = true;
                    (Unit as Player).SetTarget(closestTarget);
                    targetPos = closestTarget.Position;
                }
                //targetNear = false; // For testing wandering state

                // Adjust rotation towards target
                if (Unit.IsAlive)
                    Unit.transform.LookAt(targetPos);
                //Unit.AI.UpdateRotation = true;

                if (!Unit.IsAlive || targetNear)
                {
                    if (Unit.SpellCast.IsCasting)
                        castTimeTracker.Reset(RandomUtils.Next(1000, 1500));
                    else
                    {
                        if (Unit.IsAlive)
                        {
                            // Stop moving
                            if (Unit.HasState(UnitControlState.Wander))
                            {
                                Unit.RemoveFlag(UnitFlags.Wander);
                                Unit.RemoveState(UnitControlState.Wander);
                                Unit.Motion.ModifyWanderMovement(false);
                            }
                            // Choose spell
                            if (RandomNumb < 5)
                                Unit.GetBalance().SpellInfosById.TryGetValue(19, out newSpellInfo); // CoC
                            else if (RandomNumb < 8)
                                Unit.GetBalance().SpellInfosById.TryGetValue(7, out newSpellInfo); // Blink
                            else if (RandomNumb < 10)
                                Unit.GetBalance().SpellInfosById.TryGetValue(24, out newSpellInfo); // Block
                            else if (RandomNumb < 14)
                                Unit.GetBalance().SpellInfosById.TryGetValue(1, out newSpellInfo); // Frost nova
                            else if (RandomNumb < 15 && Unit.IsAlive && !Unit.IsMovementBlocked)
                            {
                                Unit.GetBalance().SpellInfosById.TryGetValue(8, out newSpellInfo); // Blazing speed
                                targetPos = Unit.Target.Position;
                                float diffX = Mathf.Abs(myPos.x - targetPos.x) / 2;
                                float diffY = Mathf.Abs(myPos.y - targetPos.y) / 2;
                                float diffZ = Mathf.Abs(myPos.z - targetPos.z) / 2;
                                targetPos.x = myPos.x > targetPos.x ? myPos.x - diffX : myPos.x + diffX;
                                targetPos.y = myPos.y > targetPos.y ? myPos.y - diffY : myPos.y + diffY;
                                targetPos.z = myPos.z > targetPos.z ? myPos.z - diffZ : myPos.z + diffZ;
                                //Unit.Motion.StartChargingMovement(targetPos, 15.0F); // Apply charge effect later
                            }
                            else if (RandomNumb < 18)
                            {
                                Unit.GetBalance().SpellInfosById.TryGetValue(15, out newSpellInfo); // Renew
                                (Unit as Player).SetTarget(Unit); // Target self
                                                                  //Unit.SetMovementFlag(MovementFlags.Forward, false);
                            }
                            else
                            {
                                // Requires target
                                RandomNumb = RandomGen.Next(100);
                                if (RandomNumb < 20)
                                    Unit.GetBalance().SpellInfosById.TryGetValue(20, out newSpellInfo); // Pyroblast
                                else if (RandomNumb < 40)
                                    Unit.GetBalance().SpellInfosById.TryGetValue(5, out newSpellInfo); // Frost bolt
                                else if (RandomNumb < 60)
                                    Unit.GetBalance().SpellInfosById.TryGetValue(18, out newSpellInfo); // Scorch
                                else if (RandomNumb < 80)
                                    Unit.GetBalance().SpellInfosById.TryGetValue(4, out newSpellInfo); // Fire blast
                                else if (RandomNumb < 95)
                                    Unit.GetBalance().SpellInfosById.TryGetValue(9, out newSpellInfo); // Ice lance
                                else if (RandomNumb < 97 && Unit.Target.SpellCast.IsCasting)
                                    Unit.GetBalance().SpellInfosById.TryGetValue(16, out newSpellInfo); // Counterspell
                                else
                                    Unit.GetBalance().SpellInfosById.TryGetValue(17, out newSpellInfo); // Polymorph
                            }
                        }

                        if (!Unit.IsAlive && RandomNumb < 7) // Chance to ress
                            Unit.GetBalance().SpellInfosById.TryGetValue(2, out newSpellInfo); // Ress

                        spellInfo = newSpellInfo;
                        //Debug.Log("Bot casting spell: " + spellInfo.name + ", " + spellInfo.Id);

                        Unit.Spells.CastSpell(spellInfo, new SpellCastingOptions());
                        if (spellInfo.Id == 8)
                            Unit.Motion.StartChargingMovement(targetPos, 15.0F); // Apply blazing speed charge
                        castTimeTracker.Reset(RandomUtils.Next(2000, 3500));
                    }
                }
                else
                {
                    if (!Unit.HasState(UnitControlState.Wander) && !Unit.SpellCast.IsCasting && Unit.IsAlive)
                    {
                        Unit.CurrWanderNode = 100;
                        Unit.SetFlag(UnitFlags.Wander);
                        Unit.AddState(UnitControlState.Wander);
                        Unit.Motion.ModifyWanderMovement(true);
                    }
                    //castTimeTracker.Reset(RandomUtils.Next(castIntervalMin, castIntervalMax)); // castIntervalMin = 10000
                    castTimeTracker.Reset(RandomUtils.Next(3000, 6000));
                }
            }
        }

        private Unit GetClosestPlayerTarget(Vector3 myPos, List<Unit> targets)
        {
            Unit closestTarget = null;
            float diffX, diffY, diffZ;
            float closestDiff = 100.0F;
            foreach (var target in targets)
            {
                if (target.Name.Contains("Player") && target.IsAlive)
                {
                    diffX = Mathf.Abs(myPos.x - target.Position.x);
                    diffY = Mathf.Abs(myPos.y - target.Position.y);
                    diffZ = Mathf.Abs(myPos.z - target.Position.z);
                    float currDiff = diffX + diffY + diffZ;
                    if (currDiff < closestDiff)
                    {
                        closestTarget = target;
                        closestDiff = currDiff;
                    }
                }
            }
            return closestTarget;
        }
    }
}
