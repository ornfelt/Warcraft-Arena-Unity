using Common;
using JetBrains.Annotations;
using UnityEditor.SearchService;
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

            if (!Unit.IsAlive && Unit.HasFlag(UnitFlags.Confused))
            {
                Unit.RemoveFlag(UnitFlags.Confused);
                Unit.RemoveState(UnitControlState.Confused);
                Unit.Motion.ModifyConfusedMovement(false);
            }

            if (castTimeTracker.Passed)
            {
                int RandomNumb = RandomGen.Next(100);
                SpellInfo newSpellInfo;
                bool targetNear = false;

                // Pick target
                Vector3 center = Unit.Position;
                float radius = 15.0F;
                List<Unit> targets = new List<Unit>();
                Unit.Map.SearchAreaTargets(targets, radius, center, Unit, SpellTargetChecks.Enemy);

                foreach (var target in targets)
                    if (target.Name.Contains("Player") && target.IsAlive)
                    {
                        (Unit as Player).SetTarget(target);
                        targetNear = true;
                        break;
                    }

                if (!Unit.IsAlive || targetNear)
                {
                    if (RandomNumb < 5)
                        Unit.GetBalance().SpellInfosById.TryGetValue(19, out newSpellInfo); // CoC
                    else if (RandomNumb < 8)
                        Unit.GetBalance().SpellInfosById.TryGetValue(7, out newSpellInfo); // Blink
                    else if (RandomNumb < 10)
                        Unit.GetBalance().SpellInfosById.TryGetValue(24, out newSpellInfo); // Block
                    else if (RandomNumb < 14)
                        Unit.GetBalance().SpellInfosById.TryGetValue(1, out newSpellInfo); // Frost nova
                    else if (RandomNumb < 16)
                        Unit.GetBalance().SpellInfosById.TryGetValue(8, out newSpellInfo); // Blazing speed
                    else if (RandomNumb < 19)
                    {
                        Unit.GetBalance().SpellInfosById.TryGetValue(15, out newSpellInfo); // Renew
                        (Unit as Player).SetTarget(Unit); // Target self
                        //Unit.SetMovementFlag(MovementFlags.Forward, false);
                    }
                    else
                    {
                        RandomNumb = RandomGen.Next(100);
                        if (RandomNumb < 25)
                            Unit.GetBalance().SpellInfosById.TryGetValue(20, out newSpellInfo); // Pyroblast
                        else if (RandomNumb < 50)
                            Unit.GetBalance().SpellInfosById.TryGetValue(5, out newSpellInfo); // Frost bolt
                        else if (RandomNumb < 75)
                            Unit.GetBalance().SpellInfosById.TryGetValue(4, out newSpellInfo); // Fire blast
                        else if (RandomNumb < 95)
                            Unit.GetBalance().SpellInfosById.TryGetValue(9, out newSpellInfo); // Ice lance
                        else
                            Unit.GetBalance().SpellInfosById.TryGetValue(17, out newSpellInfo); // Polymorph

                        // Stop moving
                        Unit.RemoveFlag(UnitFlags.Confused);
                        Unit.RemoveState(UnitControlState.Confused);
                        Unit.Motion.ModifyConfusedMovement(false);
                    }

                    if (!Unit.IsAlive && RandomNumb < 4) // Chance to ress
                        Unit.GetBalance().SpellInfosById.TryGetValue(2, out newSpellInfo); // Ress

                    spellInfo = newSpellInfo;
                    //Debug.Log("Bot casting spell: " + spellInfo.name + ", " + spellInfo.Id);

                    Unit.Spells.CastSpell(spellInfo, new SpellCastingOptions());
                    castTimeTracker.Reset(RandomUtils.Next(1000, 1500));
                }
                else
                {
                    if (!Unit.HasState(UnitControlState.Confused) && !Unit.SpellCast.IsCasting)
                    {
                        Unit.SetFlag(UnitFlags.Confused);
                        Unit.AddState(UnitControlState.Confused);
                        Unit.Motion.ModifyConfusedMovement(true);
                    }
                    //castTimeTracker.Reset(RandomUtils.Next(castIntervalMin, castIntervalMax)); // castIntervalMin = 10000
                    castTimeTracker.Reset(RandomUtils.Next(2000, 5000));
                }
            }
        }
    }
}
