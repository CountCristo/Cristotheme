using Automation.Forms;
using HarmonyLib;
using Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automation
{
    public class Automation : AuroraPatch.Patch
    {
        public override IEnumerable<string> Dependencies => new[] { "Lib" };

        public Economics Economics { get; private set; } = null;

        internal Lib.Lib Lib { get; private set; } = null;

        protected override void Loaded(Harmony harmony)
        {
            Economics = new Economics() { Automation = this };

            Lib = GetDependency<Lib.Lib>("Lib");
        }

        protected override void Started()
        {
            foreach (var button in Lib.KnowledgeBase.GetTimeIncrementButtons())
            {
                LogInfo($"Button {button.Name} click patched");
                button.Click += OnButtonClick;
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            LogInfo($"You've clicked a time increment button and opened the eco window");

            Lib.UIManager.RunOnForm(AuroraType.EconomicsForm, form =>
            {
                foreach (var c in UIManager.IterateControls(form))
                {
                    LogInfo($"Control type {c.GetType().Name} name {c.Name}");
                }
            });

            Economics.CreateConstructionProject("Earth", "Components", "Diplomacy Module",3 ,40);
        }
    }
}
