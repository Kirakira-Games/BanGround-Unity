using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Zenject;
using Transition = V2.Transition;

namespace BGEditor
{
    public class TimingPointEdit : MonoBehaviour
    {
        [Inject(Id = "Blocker")]
        private Button Blocker;
        [Inject]
        private IChartCore Core;

        public FloatInput Speed;
        public ColorPicker ColorPick;
        public Text BeatText;
        public Button[] Buttons;
        public Dropdown[] Dropdowns;

        private V2.TransitionColor[] Colors;
        public V2.TimingPoint Point;

        public static readonly Transition[] ColorTransitions = new Transition[]
        {
            Transition.Constant,
            Transition.Linear
        };

        private void Init()
        {
            Colors = new V2.TransitionColor[5];
            foreach (var button in Buttons)
            {
                button.onClick.AddListener(() =>
                {
                    _ = ShowPicker(button);
                });
            }
            foreach (var dropdown in Dropdowns)
            {
                dropdown.ClearOptions();
                foreach (var i in ColorTransitions)
                {
                    dropdown.options.Add(new Dropdown.OptionData(i.ToString()));
                }
                dropdown.value = 0;
            }
        }

        public async UniTaskVoid ShowPicker(Button button)
        {
            ColorPick.Initial = button.image.color;
            button.image.color = await ColorPick.Show();
        }

        public void Show()
        {
            if (gameObject.activeSelf) return;

            if (Colors == null)
            {
                Init();
            }

            BeatText.text = $"{Point.beat[0]}:{Point.beat[1]}/{Point.beat[2]}";

            // Colors
            Colors[0] = Point.tap;
            Colors[1] = Point.tapGrey;
            Colors[2] = Point.flick;
            Colors[3] = Point.slideTick;
            Colors[4] = Point.slide;

            Speed.SetValue(Point.speed);
            // Buttons
            for (int i = 0; i < Colors.Length; i++)
                Buttons[i].image.color = Colors[i];

            // Dropdowns
            for (int i = 0; i < Dropdowns.Length; i++)
            {
                for (int j = 0; j < ColorTransitions.Length; j++)
                {
                    if (ColorTransitions[j] == Colors[i].transition)
                    {
                        Dropdowns[i].value = j;
                        break;
                    }
                }
            }

            Blocker.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Save()
        {
            Point.speed.Set(Speed.value);

            for (int i = 0; i < Colors.Length; i++)
            {
                Transition trans = ColorTransitions[Dropdowns[i].value];
                Colors[i].Set(Buttons[i].image.color, trans);
            }

            Core.onTimingPointModified.Invoke();
        }

        public void Exit(bool save)
        {
            if (save)
                Save();
            gameObject.SetActive(false);
            Blocker.gameObject.SetActive(false);
        }
    }
}
