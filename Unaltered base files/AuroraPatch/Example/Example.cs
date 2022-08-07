using System;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;

using HarmonyLib;
using AuroraPatch;
using Lib;
using System.Data;

namespace Example
{
    public class Example : AuroraPatch.Patch
    {
        public static Color BackColor { get; set; } = Color.Black;

        public override string Description => "An example patch.";
        public override IEnumerable<string> Dependencies => new[] { "Lib" };

        protected override void Loaded(Harmony harmony)
        {
            LogInfo("Loading ExamplePatch...");

            try
            {
                BackColor = Deserialize<Color>("color");
            }
            catch
            {
                LogInfo("saved color not found");
            }

            // get the exe and its checksum
            var exe = AuroraExecutablePath;
            var checksum = AuroraChecksum;

            // dependency
            var lib = GetDependency<Lib.Lib>("Lib");
            var map = lib.SignatureManager.Get(AuroraType.TacticalMapForm);

            // add event handler
            lib.RegisterEventHandler(AuroraType.ClassDesignForm, "Shown", GetType().GetMethod("OnClassDesignFormShown", AccessTools.all));

            // Harmony
            var ctor = (MethodBase)map.GetMember(".ctor", AccessTools.all)[0];
            var method = new HarmonyMethod(GetType().GetMethod("PatchTacticalMapConstructor", AccessTools.all));
            harmony.Patch(ctor, null, method);
        }

        protected override void Started()
        {
            var message = "Example patch loaded!\n";

            // read in-memory db
            var lib = GetDependency<Lib.Lib>("Lib");
            var table = lib.DatabaseManager.ExecuteQuery("SELECT RaceName FROM FCT_Race");

            foreach (DataRow row in table.Rows)
            {
                message += $"Race name: {row[0]}\n";
            }

            // get open forms
            var forms = lib.GetOpenForms();
            foreach (var form in forms)
            {
                message += $"Form name: {form.Name}";
            }

            // invoke arbitrary code on Aurora's UI thread
            var action = new Action(() => MessageBox.Show(message));
            InvokeOnUIThread(action);
        }

        protected override void ChangeSettings()
        {
            // pick TacticalMap background color

            var diag = new ColorDialog();
            diag.Color = BackColor;
            
            var result = diag.ShowDialog();
            if (result == DialogResult.OK)
            {
                BackColor = diag.Color;

                if (TacticalMap != null)
                {
                    var action = new Action(() => TacticalMap.BackColor = BackColor);
                    InvokeOnUIThread(action);
                }

                Serialize("color", BackColor);
            }
        }

        private static void PatchTacticalMapConstructor(Form __instance)
        {
            // set background color to black
            __instance.BackColor = BackColor;

            MessageBox.Show("Harmony patched!");
        }

        private static void OnClassDesignFormShown(object sender, EventArgs e)
        {
            MessageBox.Show("You've opened the class design");
        }
    }

}
