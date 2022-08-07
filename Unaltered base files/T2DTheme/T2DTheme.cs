using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using HarmonyLib;
using Lib;

namespace T2DTheme
{
    public class T2DTheme : AuroraPatch.Patch
    {
        public override string Description => "twice2double's Dark Aurora Theme";
        public override IEnumerable<string> Dependencies => new[] { "ThemeCreator" };

        // The global font used.
        public static Font font = null;

        protected override void Loaded(Harmony harmony)
        {
            LogInfo("Loading T2DTheme...");

            // -- Settings -- //
            try
            {
                font = Deserialize<Font>("font");
            }
            catch (Exception)
            {
                LogInfo("Saved font not found");
            }

            // -- Fonts -- //
            if (font != null) ThemeCreator.ThemeCreator.AddFontChange(font);

            // -- Colors -- //
            // Black background.
            ThemeCreator.ThemeCreator.AddColorChange(Color.FromArgb(0, 0, 64), Color.Black);
            // Black Buttons.
            ThemeCreator.ThemeCreator.AddColorChange(typeof(Button), new ThemeCreator.ColorChange { BackgroundColor = Color.Black });
            // White foreground.
            ThemeCreator.ThemeCreator.AddColorChange(Color.FromArgb(255, 255, 192), Color.White);

            // -- Images -- //
            var lib = GetDependency<Lib.Lib>("Lib");
            string imagePath = @"Patches\T2DTheme\Icons\";
            List<string> icons = Directory.EnumerateFiles(imagePath).ToList();
            foreach (KeyValuePair<AuroraButton, string> auroraButtons in lib.KnowledgeBase.GetKnownButtonNames())
            {
                string iconPath = imagePath + auroraButtons.Value + ".BackgroundImage.png";
                if (icons.Contains(iconPath))
                {
                    ThemeCreator.ThemeCreator.AddImageChange(auroraButtons.Key, iconPath);
                }
            }

            // -- Graphics -- //
            ThemeCreator.ThemeCreator.SetMapTextColor(Color.White);
            ThemeCreator.ThemeCreator.SetDistanceRulerLineColor(Color.Gray);
            ThemeCreator.ThemeCreator.SetOrbitColor(Color.Gray);
            ThemeCreator.ThemeCreator.SetPlanetColor(Color.Gray);
            ThemeCreator.ThemeCreator.SetStarSystemColor(Color.Gray);
        }

        protected override void ChangeSettings()
        {
            var dialog = new FontDialog();
            if (font != null) dialog.Font = font;
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                font = dialog.Font;
                Serialize("font", font);
            }
        }
    }
}
