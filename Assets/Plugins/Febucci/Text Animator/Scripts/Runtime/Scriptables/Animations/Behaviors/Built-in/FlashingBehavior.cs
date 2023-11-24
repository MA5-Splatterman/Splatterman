using Febucci.UI.Core;
using UnityEngine;

namespace Febucci.UI.Effects
{
    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = "Text Animator/Animations/Behaviors/Flashing", fileName = "Flashing Behavior")]
    [EffectInfo("flash", EffectCategory.Behaviors)]
    public sealed class FlashingBehavior : BehaviorScriptableBase
    {
        public float baseFrequency = 0.5f;
        public Color baseColor;


        Color color;
        float frequency;
        public override void SetModifier(ModifierInfo modifier)
        {
            switch (modifier.name)
            {
                case "c": color = baseColor * modifier.value; break;
                //frequency
                case "f": frequency = baseFrequency * modifier.value; break;
            }
        }

        public override void ResetContext(TAnimCore animator)
        {
            color = baseColor;
            frequency = baseFrequency;
        }

        Color32 temp;
        public override void ApplyEffectTo(ref Core.CharacterData character, TAnimCore animator)
        {
            for (byte i = 0; i < TextUtilities.verticesPerChar; i++)
            {
                //shifts hue
                temp = Color.HSVToRGB(1, 1, Mathf.PingPong(animator.time.timeSinceStart * frequency, 1));
                temp.a = character.current.colors[i].a; //preserves original alpha
                character.current.colors[i] = temp;
            }
        }
    }
}