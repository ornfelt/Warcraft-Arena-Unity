using Common;
using JetBrains.Annotations;
using UnityEngine;

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
            if (castTimeTracker.Passed)
            {
                int RandomNumb = RandomGen.Next(100);
                SpellInfo newSpellInfo;
                if (RandomNumb < 30)
                    Unit.GetBalance().SpellInfosById.TryGetValue(19, out newSpellInfo); // CoC
                else if (RandomNumb < 40)
                    Unit.GetBalance().SpellInfosById.TryGetValue(7, out newSpellInfo); // Blink
                else if (RandomNumb < 45)
                    Unit.GetBalance().SpellInfosById.TryGetValue(24, out newSpellInfo); // Block
                else if (RandomNumb < 65)
                    //Unit.GetBalance().SpellInfosById.TryGetValue(1, out newSpellInfo); // Frost nova
                    Unit.GetBalance().SpellInfosById.TryGetValue(2, out newSpellInfo); // Ress
                else if (RandomNumb < 85)
                {
                    Unit.GetBalance().SpellInfosById.TryGetValue(8, out newSpellInfo); // Blazing speed
                    //Unit.SetMovementFlag(MovementFlags.Forward, true);
                }
                else
                {
                    Unit.GetBalance().SpellInfosById.TryGetValue(2, out newSpellInfo); // Ress
                    Unit.SetMovementFlag(MovementFlags.Forward, false);
                }

                spellInfo = newSpellInfo;
                if (spellInfo.Id == 2)
                    Debug.Log("Bot casting spell: " + spellInfo.name + ", " + spellInfo.Id);

                Unit.Spells.CastSpell(spellInfo, new SpellCastingOptions());
                castTimeTracker.Reset(RandomUtils.Next(castIntervalMin, castIntervalMax));
            }

            if (!Unit.HasState(UnitControlState.Confused))
            {
                Unit.SetFlag(UnitFlags.Confused);
                Unit.AddState(UnitControlState.Confused);
                Unit.Motion.ModifyConfusedMovement(true);
            }
        }
    }
}
