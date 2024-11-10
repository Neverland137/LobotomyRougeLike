using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewGameMode
{
    public static class ButtonInteraction_Data
    {
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

        public static Sprite originSprite = new Sprite();
        public static UnityEngine.Color originColor = new UnityEngine.Color();
        public static float time = 0;
        public static Sprite sp;
        public static SpriteRenderer frontsprite;
        public static GameObject gameobj;
        public static float fadeprogress = 0f;
        public static ButtonState currentState = ButtonState.Idle;
    }
    public enum ButtonState
    {
        Idle,
        FadingIn,
        FadingOut
    }
}
