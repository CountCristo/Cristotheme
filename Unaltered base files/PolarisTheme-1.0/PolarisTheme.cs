using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

using HarmonyLib;
using Lib;
using PolarisTheme.Properties;

namespace PolarisTheme
{
    public class PolarisTheme : AuroraPatch.Patch
    {
        public override string Description => "A configurable light-colored theme";
        public override IEnumerable<string> Dependencies => new[] { "ThemeCreator", "Lib" };
        private static Lib.Lib lib;
        private static bool lockSettings = false;

        // memory
        private static string lastActiveTimeIncrement;
        private static string activeSubPulse;

        // Old colors
        private static readonly Color oldCyan = Color.FromArgb(0, 255, 255);
        private static readonly Color oldMapColor = Color.FromArgb(0, 0, 64);
        private static readonly Color oldLightYellow = Color.FromArgb(255, 255, 192);
        private static readonly Color oldLightGreen = Color.FromArgb(128, 255, 128);
        private static readonly Color oldButtonHighlight = Color.FromArgb(0, 0, 120);

        // settings controlled by user
        private static bool cfg_antiAlias;
        private static bool cfg_no_dropdown_color;
        private static Color cfg_mapColor;
        private static Color cfg_textColor;
        private static Color cfg_gray_221;
        private static Color cfg_combo_box_color;
        private static Color cfg_light_blue_highlight;
        private static Color cfg_darker_orange;
        private static Color cfg_not_light_green_anymore;
        private static Color cfg_not_cyan_anymore;
        private static Color cfg_not_cyan_body_anymore;
        private static Color cfg_not_dodger_blue_body_anymore;
        private static Color cfg_not_chocolate_anymore;
        private static Color cfg_not_light_gray_anymore;

        // defaults for the configurable settings
        private void ResetToDefaultSettings()
        {
            cfg_antiAlias = true;
            cfg_no_dropdown_color = false;
            cfg_mapColor = oldMapColor;
            cfg_textColor = Color.Black;
            cfg_gray_221 = Color.FromArgb(221, 221, 221);
            cfg_combo_box_color = Color.FromArgb(240, 240, 240);
            cfg_light_blue_highlight = Color.FromArgb(100, 216, 255);
            cfg_darker_orange = Color.FromArgb(187, 96, 0);
            cfg_not_light_green_anymore = Color.FromArgb(0, 0, 160);
            cfg_not_cyan_anymore = Color.Teal;
            cfg_not_cyan_body_anymore = Color.FromArgb(0, 119, 255);
            cfg_not_dodger_blue_body_anymore = Color.Blue;
            cfg_not_chocolate_anymore = Color.FromArgb(178, 34, 34);
            cfg_not_light_gray_anymore = Color.FromArgb(90, 90, 120);
        }


        // what should stay blue
        private static List<String> mapNames = new List<String> { "TacticalMap", "GalacticMap" };
        private static List<String> otherBlueControls = new List<String> { "panJP" };
        private static bool ShouldRevertToBlue(Control control)
        {
            return mapNames.Contains(control.Name) || otherBlueControls.Contains(control.Name)
                || control.Parent != null && mapNames.Contains(control.Parent.Name);
        }

        //
        // settings code for the aurora patcher
        //

        private void RefreshSettingsUI(Settings form)
        {
            form.antiAlias.Checked = cfg_antiAlias;
            form.no_dropdown_color.Checked = cfg_no_dropdown_color;
            form.mapColor.BackColor = cfg_mapColor;
            form.textColor.BackColor = cfg_textColor;
            form.gray_221.BackColor = cfg_gray_221;
            form.combo_box_color.BackColor = cfg_combo_box_color;
            form.light_blue_highlight.BackColor = cfg_light_blue_highlight;
            form.darker_orange.BackColor = cfg_darker_orange;
            form.not_light_green_anymore.BackColor = cfg_not_light_green_anymore;
            form.not_cyan_anymore.BackColor = cfg_not_cyan_anymore;
            form.not_cyan_body_anymore.BackColor = cfg_not_cyan_body_anymore;
            form.not_dodger_blue_body_anymore.BackColor = cfg_not_dodger_blue_body_anymore;
            form.not_chocolate_anymore.BackColor = cfg_not_chocolate_anymore;
            form.not_light_gray_anymore.BackColor = cfg_not_light_gray_anymore;
            RefreshPreview(form);
        }

        private void RefreshPreview(Settings form)
        {
            form.preview_mapColor1.BackColor = form.mapColor.BackColor;
            form.preview_textColor1.ForeColor = form.textColor.BackColor;
            form.preview_textColor2.ForeColor = form.textColor.BackColor;
            form.preview_gray_2211.BackColor = form.gray_221.BackColor;
            form.preview_combo_box_color1.BackColor = form.combo_box_color.BackColor;
            form.preview_combo_box_color2.BackColor = form.combo_box_color.BackColor;
            form.preview_combo_box_color1.ForeColor = form.textColor.BackColor;
            form.preview_combo_box_color2.ForeColor = form.textColor.BackColor;
            form.preview_combo_box_color1.DropDownStyle = form.no_dropdown_color.Checked ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
            form.preview_combo_box_color2.DropDownStyle = form.no_dropdown_color.Checked ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
            form.preview_time_button1.BackColor = form.gray_221.BackColor;
            form.preview_time_button2.BackColor = form.light_blue_highlight.BackColor;
            form.preview_time_button1.ForeColor = form.textColor.BackColor;
            form.preview_time_button2.ForeColor = form.textColor.BackColor;
            form.preview_darker_orange1.ForeColor = form.darker_orange.BackColor;
            form.preview_not_light_green_anymore1.ForeColor = form.not_light_green_anymore.BackColor;
            form.preview_not_light_green_anymore2.ForeColor = form.not_light_green_anymore.BackColor;
            form.preview_not_light_green_anymore3.ForeColor = form.not_light_green_anymore.BackColor;
            form.preview_not_cyan_anymore1.ForeColor = form.not_cyan_anymore.BackColor;
            form.preview_not_cyan_body_anymore1.ForeColor = form.not_cyan_body_anymore.BackColor;
            form.preview_not_dodger_blue_body_anymore1.ForeColor = form.not_dodger_blue_body_anymore.BackColor;
            form.preview_not_chocolate_anymore1.ForeColor = form.not_chocolate_anymore.BackColor;
            form.preview_not_light_gray_anymore1.ForeColor = form.not_light_gray_anymore.BackColor;
        }
        private void ResetToAuroraSettings() // these are not the mod defaults!  Scroll up!
        {
            cfg_antiAlias = false;
            cfg_no_dropdown_color = false;
            cfg_mapColor = oldMapColor;
            cfg_textColor = oldLightYellow;
            cfg_gray_221 = oldMapColor;
            cfg_combo_box_color = oldMapColor;
            cfg_light_blue_highlight = oldButtonHighlight;
            cfg_darker_orange = Color.Orange;
            cfg_not_light_green_anymore = oldLightGreen;
            cfg_not_cyan_anymore = oldCyan;
            cfg_not_cyan_body_anymore = Color.Cyan;
            cfg_not_dodger_blue_body_anymore = Color.DodgerBlue;
            cfg_not_chocolate_anymore = Color.Chocolate;
            cfg_not_light_gray_anymore = Color.LightGray;
        }


        private void OnColorButtonClick(Button button)
        {
            var diag = new ColorDialog();
            diag.Color = button.BackColor;

            if (diag.ShowDialog() == DialogResult.OK)
            {
                button.BackColor = diag.Color;
                RefreshPreview(button.Parent as Settings);
            }
        }

        protected override void ChangeSettings()
        {
            if (lockSettings)
            {
                MessageBox.Show("Note: changes won't take effect until you restart Aurora");
            }
            var form = new Settings();
            form.preview_combo_box_color1.Items.Add("Terran Federation");
            form.preview_combo_box_color2.Items.Add("Terran Federation");
            form.preview_combo_box_color1.SelectedIndex = 0;
            form.preview_combo_box_color2.SelectedIndex = 0;
            LoadSettings();
            RefreshSettingsUI(form);

            form.resetAll.Click += (object sender, EventArgs e) =>
            {
                ResetToDefaultSettings();
                RefreshSettingsUI(form);
            };

            form.resetToAurora.Click += (object sender, EventArgs e) =>
            {
                ResetToAuroraSettings();
                RefreshSettingsUI(form);
            };

            form.no_dropdown_color.CheckedChanged += (object sender, EventArgs e) => RefreshPreview(form);
            form.mapColor.Click += (object sender, EventArgs e) => OnColorButtonClick(form.mapColor);
            form.textColor.Click += (object sender, EventArgs e) => OnColorButtonClick(form.textColor);
            form.gray_221.Click += (object sender, EventArgs e) => OnColorButtonClick(form.gray_221);
            form.combo_box_color.Click += (object sender, EventArgs e) => OnColorButtonClick(form.combo_box_color);
            form.light_blue_highlight.Click += (object sender, EventArgs e) => OnColorButtonClick(form.light_blue_highlight);
            form.darker_orange.Click += (object sender, EventArgs e) => OnColorButtonClick(form.darker_orange);
            form.not_light_green_anymore.Click += (object sender, EventArgs e) => OnColorButtonClick(form.not_light_green_anymore);
            form.not_cyan_anymore.Click += (object sender, EventArgs e) => OnColorButtonClick(form.not_cyan_anymore);
            form.not_cyan_body_anymore.Click += (object sender, EventArgs e) => OnColorButtonClick(form.not_cyan_body_anymore);
            form.not_dodger_blue_body_anymore.Click += (object sender, EventArgs e) => OnColorButtonClick(form.not_dodger_blue_body_anymore);
            form.not_chocolate_anymore.Click += (object sender, EventArgs e) => OnColorButtonClick(form.not_chocolate_anymore);
            form.not_light_gray_anymore.Click += (object sender, EventArgs e) => OnColorButtonClick(form.not_light_gray_anymore);

            form.saveButton.Click += (object sender, EventArgs e) => {
                cfg_antiAlias = form.antiAlias.Checked;
                cfg_no_dropdown_color = form.no_dropdown_color.Checked;
                cfg_mapColor = form.mapColor.BackColor;
                cfg_textColor = form.textColor.BackColor;
                cfg_gray_221 = form.gray_221.BackColor;
                cfg_combo_box_color = form.combo_box_color.BackColor;
                cfg_light_blue_highlight = form.light_blue_highlight.BackColor;
                cfg_darker_orange = form.darker_orange.BackColor;
                cfg_not_light_green_anymore = form.not_light_green_anymore.BackColor;
                cfg_not_cyan_anymore = form.not_cyan_anymore.BackColor;
                cfg_not_cyan_body_anymore = form.not_cyan_body_anymore.BackColor;
                cfg_not_dodger_blue_body_anymore = form.not_dodger_blue_body_anymore.BackColor;
                cfg_not_chocolate_anymore = form.not_chocolate_anymore.BackColor;
                cfg_not_light_gray_anymore = form.not_light_gray_anymore.BackColor;
                Serialize("antiAlias", cfg_antiAlias);
                Serialize("no_dropdown_color", cfg_no_dropdown_color);
                Serialize("mapColor", cfg_mapColor);
                Serialize("textColor", cfg_textColor);
                Serialize("gray_221", cfg_gray_221);
                Serialize("combo_box_color", cfg_combo_box_color);
                Serialize("light_blue_highlight", cfg_light_blue_highlight);
                Serialize("darker_orange", cfg_darker_orange);
                Serialize("not_light_green_anymore", cfg_not_light_green_anymore);
                Serialize("not_cyan_anymore", cfg_not_cyan_anymore);
                Serialize("not_cyan_body_anymore", cfg_not_cyan_body_anymore);
                Serialize("not_dodger_blue_body_anymore", cfg_not_dodger_blue_body_anymore);
                Serialize("not_chocolate_anymore", cfg_not_chocolate_anymore);
                Serialize("not_light_gray_anymore", cfg_not_light_gray_anymore);
                if (lockSettings)
                {
                    MessageBox.Show("Settings saved, but they won't take effect until you restart Aurora");
                }
                form.Close();
                form.Dispose();
            };
            form.ShowDialog();
        }

        private void LoadSettings()
        {
            ResetToDefaultSettings();
            if (File.Exists(GetSettingFileName("antiAlias"))) cfg_antiAlias = Deserialize<bool>("antiAlias");
            if (File.Exists(GetSettingFileName("no_dropdown_color"))) cfg_no_dropdown_color = Deserialize<bool>("no_dropdown_color");
            if (File.Exists(GetSettingFileName("mapColor"))) cfg_mapColor = Deserialize<Color>("mapColor");
            if (File.Exists(GetSettingFileName("textColor"))) cfg_textColor = Deserialize<Color>("textColor");
            if (File.Exists(GetSettingFileName("gray_221"))) cfg_gray_221 = Deserialize<Color>("gray_221");
            if (File.Exists(GetSettingFileName("combo_box_color"))) cfg_combo_box_color = Deserialize<Color>("combo_box_color");
            if (File.Exists(GetSettingFileName("light_blue_highlight"))) cfg_light_blue_highlight = Deserialize<Color>("light_blue_highlight");
            if (File.Exists(GetSettingFileName("darker_orange"))) cfg_darker_orange = Deserialize<Color>("darker_orange");
            if (File.Exists(GetSettingFileName("not_light_green_anymore"))) cfg_not_light_green_anymore = Deserialize<Color>("not_light_green_anymore");
            if (File.Exists(GetSettingFileName("not_cyan_anymore"))) cfg_not_cyan_anymore = Deserialize<Color>("not_cyan_anymore");
            if (File.Exists(GetSettingFileName("not_cyan_body_anymore"))) cfg_not_cyan_body_anymore = Deserialize<Color>("not_cyan_body_anymore");
            if (File.Exists(GetSettingFileName("not_dodger_blue_body_anymore"))) cfg_not_dodger_blue_body_anymore = Deserialize<Color>("not_dodger_blue_body_anymore");
            if (File.Exists(GetSettingFileName("not_chocolate_anymore"))) cfg_not_chocolate_anymore = Deserialize<Color>("not_chocolate_anymore");
            if (File.Exists(GetSettingFileName("not_light_gray_anymore"))) cfg_not_light_gray_anymore = Deserialize<Color>("not_light_gray_anymore");
        }

        private string GetSettingFileName(string setting)
        {
            return Path.Combine(Path.GetDirectoryName(AuroraExecutablePath), "Patches", Name, setting + ".json");
        }

        //
        // begin patching aurora
        //

        protected override void Loaded(Harmony harmony)
        {
            //Harmony.DEBUG = true;   // logging
            lockSettings = true;
            FileLog.Reset();
            LogInfo("Loading PolarisTheme");
            
            
            lib = GetDependency<Lib.Lib>("Lib");
            
            LoadSettings();

            var revertToBlue = new ThemeCreator.ColorChange { BackgroundColor = cfg_mapColor };
            var setToGray221 = new ThemeCreator.ColorChange { BackgroundColor = cfg_gray_221 };
            lastActiveTimeIncrement = lib.KnowledgeBase.GetButtonName(AuroraButton.Increment);
            activeSubPulse = lib.KnowledgeBase.GetButtonName(AuroraButton.SubPulse); 

            
            // blue to gray
            ThemeCreator.ThemeCreator.AddColorChange(oldMapColor, cfg_gray_221);

            // except for maps
            ThemeCreator.ThemeCreator.AddColorChange(ShouldRevertToBlue, revertToBlue);

            // fix blue bar on system view
            ThemeCreator.ThemeCreator.AddColorChange("lstvSB", setToGray221);

            
            // Patch all Color.FromArgb overloads for color overrides
            var colorConstructorPostfix = new HarmonyMethod(GetType().GetMethod("ColorConstructorPostfix", AccessTools.all));
            foreach (var method in typeof(Color).GetMethods(AccessTools.all))
            {
                if (method.Name == "FromArgb")
                {
                    harmony.Patch(method, postfix: colorConstructorPostfix);
                }
            }


            // tweak new ListViewItems after they are added to a list, such as planets in the System View window
            
            harmony.Patch(AccessTools.Method(typeof(ListView.ListViewItemCollection), "Add", new[] { typeof(ListViewItem) }),
                postfix: new HarmonyMethod(GetType().GetMethod("ListItemPostfix", AccessTools.all)));

            // tweak TreeNodes after they've been added to a Tree (so we can darken the orange in the list of commanders, etc)
            harmony.Patch(AccessTools.Method(typeof(TreeNodeCollection), "Add", new[] { typeof(TreeNode) }),
                prefix: new HarmonyMethod(GetType().GetMethod("AddTreeNodePrefix", AccessTools.all)));
            
            // Hook into Aurora forms constructors for some more advanced overrides
            var formConstructorPostfix = new HarmonyMethod(GetType().GetMethod("FormConstructorPostfix", AccessTools.all));
            foreach (var form in AuroraAssembly.GetTypes().Where(t => typeof(Form).IsAssignableFrom(t)))
            {
                foreach (var ctor in form.GetConstructors())
                {
                    harmony.Patch(ctor, postfix: formConstructorPostfix);
                }
            }
            // antialiasing and set colors back to yellow on maps
            ThemeCreator.ThemeCreator.DrawStringPrefixAction((graphics, s, font, brush) =>
            {
                if (brush.GetType() == typeof(SolidBrush))
                {
                    var solidBrush = brush as SolidBrush;

                    if (solidBrush.Color == cfg_textColor)
                    {
                        solidBrush.Color = oldLightYellow;
                    }
                }
            });

            ThemeCreator.ThemeCreator.DrawEllipsePrefixAction((graphics, pen) =>
            {
                if (cfg_antiAlias)
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                }
            });

            ThemeCreator.ThemeCreator.FillEllipsePrefixAction((graphics, brush) =>
            {
                if (cfg_antiAlias)
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                }

                if (brush.GetType() == typeof(SolidBrush))
                {
                    var solidBrush = brush as SolidBrush;

                    // This is being overriden by global color contructor hook, but we want to keep
                    // the old yellow color for player contacts, so restore.
                    if (solidBrush.Color == null || solidBrush.Color == cfg_textColor)
                    {
                        solidBrush.Color = oldLightYellow;
                    }
                }
            });

            ThemeCreator.ThemeCreator.DrawLinePrefixAction((graphics, pen) =>
            {
                if (cfg_antiAlias)
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                }
            });
            
            FileLog.Log("Polaris done patching");
        }
        // end of the patching



        private static void ColorConstructorPostfix(ref Color __result)
        {
            if (__result == oldLightYellow)
            {
                __result = cfg_textColor;
            }
            else if (__result == oldLightGreen)
            { 
                __result = cfg_not_light_green_anymore;
            }
            else if (__result == Color.LightGray)
            {
                __result = cfg_not_light_gray_anymore;
            }
        }

        private static void ListItemPostfix(ref ListViewItem __result)
        {
            if (__result.SubItems != null)
            {
                foreach (ListViewItem.ListViewSubItem subItem in __result.SubItems)
                {
                    subItem.ForeColor = GetReplacementColor(subItem.ForeColor);
                }
            }
        }

        private static void AddTreeNodePrefix(ref TreeNode node)
        {
            node.ForeColor = GetReplacementColor(node.ForeColor);
        }

        private static Color GetReplacementColor(Color origColor)
        {
            if (origColor == oldCyan)  // in list of populations on econ window
            {
                return cfg_not_cyan_anymore;
            }
            else if (origColor == Color.Cyan)  // bodies on system view
            {
                return cfg_not_cyan_body_anymore; 
            }
            else if (origColor == Color.DodgerBlue) // bodies on system view
            {
                return cfg_not_dodger_blue_body_anymore;
            }
            else if (origColor == Color.Orange) // in commander list
            {
                return cfg_darker_orange;
            }
            else if (origColor == Color.Chocolate) // bodies on system view
            {
                return cfg_not_chocolate_anymore;
            }
            else if (origColor == Color.LightGray) // seen in some lists in commanders window
            {
                return cfg_not_light_gray_anymore;
            }
            else
            {
                return origColor;
            }
        }

        private static void FormConstructorPostfix(Form __instance)
        {
            __instance.HandleCreated += (Object sender, EventArgs e) =>
            {
                IterateControls((Control)sender);
            };
        }

        private static void IterateControls(Control control)
        {
            ApplyChanges(control);

            foreach (Control childControl in control.Controls)
            {
                IterateControls(childControl);
            }
        }

        private static void ApplyChanges(Control control)
        {
            if (control.GetType() == typeof(Button))
            {
                ApplyButtonChanges(control as Button);
            }
            else if (control.GetType() == typeof(ListView))
            {
                ApplyListViewChanges(control as ListView);
            }
            else if (control.GetType() == typeof(ComboBox))
            {
                ApplyComboBoxChanges(control as ComboBox);
            }
            else if (control is Form)
            {
                ApplyFormChanges(control as Form);
            }
        }



        private static void ApplyButtonChanges(Button button)
        {
            // add a nice light blue highlight on button mouseover. also required for ApplyActiveButtonStyle to work
            button.UseVisualStyleBackColor = true;

            if (IsTimeIncrementButton(button))
            {
                button.Click += OnTimeIncrementButtonClick;
                ApplyActiveButtonStyle(button, button.Name == lastActiveTimeIncrement);
            }
            else if (IsSubPulseButton(button))
            {
                button.Click += OnSubPulseButtonClick;
                ApplyActiveButtonStyle(button, button.Name == activeSubPulse);
            }
        }


        private static void ApplyListViewChanges(ListView listView)
        {
            if (listView.View == View.Details && listView.Columns.Count > 1)
            {
                listView.FullRowSelect = true;
            }

            if (listView.Name == "lstvSB")  // fix the blue bar in the system view window
            {
                listView.OwnerDraw = false;
            }
        }

        // highlight the active time inc button on maps
        private static void OnTimeIncrementButtonClick(Object sender, EventArgs e)
        {
            var button = sender as Button;
            lastActiveTimeIncrement = button.Name;

            List<Button> timeIncrementButtons = lib.KnowledgeBase.GetTimeIncrementButtons().ToList();
            timeIncrementButtons.AddRange(lib.KnowledgeBase.GetTimeIncrementButtonsGalacticMap().ToList());

            foreach (Button timeIncrementButton in timeIncrementButtons)
            {
                ApplyActiveButtonStyle(timeIncrementButton, timeIncrementButton.Name == lastActiveTimeIncrement);
            }
        }

        // highlight the active sub pulse button on maps
        private static void OnSubPulseButtonClick(Object sender, EventArgs e)
        {
            var button = sender as Button;
            activeSubPulse = button.Name;

            foreach (Button subPulseButton in lib.KnowledgeBase.GetSubPulseButtons())
            {
                ApplyActiveButtonStyle(subPulseButton, subPulseButton.Name == activeSubPulse);
            }
        }

        private static void ApplyActiveButtonStyle(Button button, bool isActive)
        {
            button.BackColor = isActive ? cfg_light_blue_highlight : cfg_gray_221;
        }

        private static void ApplyFormChanges(Form form)
        {
            form.ShowIcon = false; // Aurora uses default Windows Forms icons
        }

        private static void ApplyComboBoxChanges(ComboBox comboBox)
        {
            comboBox.DropDownStyle = cfg_no_dropdown_color ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
            comboBox.BackColor = cfg_combo_box_color;
        }


        private static bool IsTimeIncrementButton(Button button)
        {
            return button.Name.StartsWith("cmdIncrement");
        }

        private static bool IsSubPulseButton(Button button)
        {
            return button.Name.StartsWith("cmdSubPulse");
        }

    }
}


        // ----------- trashcan for old or unused snippets below --------
        //


            
            // Also hook into some predefined/named color properties
            // warning: can cause a crash, started happening when i implemented the settings window
            //harmony.Patch(typeof(Color).GetMethod("get_LightGray"), postfix: colorConstructorPostfix);
            

        // Fonts
        //private static readonly FontFamily fontFamily = new FontFamily("Tahoma");
        //private static readonly Font mainFont = new Font(fontFamily, 8.25f);
        //private static readonly Font singleLineTextBoxFont = new Font(fontFamily, 8);
        //private static readonly Font buttonFont = new Font(fontFamily, 7, FontStyle.Bold);

        // Our new colors
        //private static readonly Color mainBackgroundColor = Color.FromArgb(12, 12, 12);
        //private static readonly Color mainTextColor = Color.FromArgb(210, 210, 210);
        //private static readonly Color disabledTextColor = ControlPaint.Dark(mainTextColor, 0.1f);
        //private static readonly Color buttonBackgroundColor = Color.FromArgb(23, 26, 39);
        //private static readonly Color planetColor = Color.FromArgb(128, 128, 128);
        //private static readonly Color orbitColor = Color.FromArgb(128, planetColor);
        //private static readonly Color enabledSpaceMasterButtonColor = Color.FromArgb(248, 231, 28);
        //private static readonly Color enabledAutoTurnsButtonColor = Color.FromArgb(126, 211, 33);

        // Toolbar button background colors
        //private static readonly Color economicsButtonBackgroundColor = Color.FromArgb(26, 45, 46);
        //private static readonly Color designButtonBackgroundColor = Color.FromArgb(41, 44, 46);
        //private static readonly Color fleetButtonBackgroundColor = Color.FromArgb(45, 26, 26);
        //private static readonly Color groundForcesButtonBackgroundColor = Color.FromArgb(42, 45, 28);
        //private static readonly Color intelligenceButtonBackgroundColor = Color.FromArgb(47, 38, 47);
        //private static readonly Color explorationButtonBackgroundColor = Color.FromArgb(24, 27, 78);
        //private static readonly Color personnelButtonBackgroundColor = Color.FromArgb(18, 41, 58);
        //private static readonly Color surveyButtonBackgroundColor = Color.FromArgb(40, 31, 24);
        //private static readonly Color technologyButtonBackgroundColor = Color.FromArgb(42, 22, 45);
        //private static readonly Color sectorButtonBackgroundColor = Color.FromArgb(20, 45, 31);

        //private static readonly Color oldPlayerContactColor = Color.FromArgb(255, 255, 192);
        //private static readonly Color oldNeutralContactColor = Color.FromArgb(144, 238, 144);
        //private static readonly Color oldCivilianContactColor = Color.FromArgb(0, 206, 209);
        //private static readonly Color oldHostileContactColor = Color.FromArgb(255, 0, 0);
        //private static readonly Color oldCometPathColor = Color.LimeGreen;
        //private static readonly Color oldOrbitColor = Color.LimeGreen;
        //private static readonly Color oldTextColor = Color.FromArgb(255, 255, 192);
        //private static readonly Color oldDisabledTextColor = Color.LightGray;
        //private static readonly Color oldEnabledButtonBackgroundColor = Color.FromArgb(0, 0, 120);
        
        // other unused colors
        //private static readonly Color c_31_37_49 = Color.FromArgb(31, 37, 49);
        //private static readonly Color defaultFontColor = Color.FromArgb(203, 205, 211);
        //private static readonly Color unusedColor = Color.FromArgb(174, 109, 176);
        //private static void ApplyListViewChanges(ListView listView)
        //{
            //if (listView.BorderStyle == BorderStyle.Fixed3D)
            //{
            //    listView.BorderStyle = BorderStyle.FixedSingle;
            //}

            //form.saveButton.Click += (object sender, EventArgs e) => {
                //if (form.cultureSettingsDropdown.SelectedItem != null)
                //{
                //    Culture = form.cultureSettingsDropdown.SelectedItem.ToString();
                //    Serialize("local", form.cultureSettingsDropdown.SelectedItem.ToString());
                //}
        
        //private static void ApplyButtonChanges(Button button)
        //{
        //    button.FlatStyle = FlatStyle.Flat;
        //    button.FlatAppearance.BorderColor = mainBackgroundColor;
        //    button.FlatAppearance.BorderSize = 2;

        //    // With some exceptions just enable auto size for buttons (necessary due to custom font)
        //    if (button.Name != lib.KnowledgeBase.GetButtonName(AuroraButton.SubPulse)
        //        && button.Name != lib.KnowledgeBase.GetButtonName(AuroraButton.Increment))
        //    {
        //        button.AutoSize = true;
        //    }



        //    else if (IsSpaceMasterButton(button))
        //    {
        //        ApplySpaceMasterButtonStyle(button);
        //        button.BackgroundImageChanged += OnSpaceMasterButtonBackgroundImageChanged;
        //    }
        //    else if (IsAutoTurnsButton(button))
        //    {
        //        ApplyAutoTurnsButtonStyle(button);
        //        button.BackgroundImageChanged += OnAutoTurnsButtonBackgroundImageChanged;
        //    }


        //private static void OnSpaceMasterButtonBackgroundImageChanged(Object sender, EventArgs e)
        //{
        //    var button = sender as Button;

        //    button.BackgroundImageChanged -= OnSpaceMasterButtonBackgroundImageChanged;

        //    // NOTE: This guard is needed as you can have both tactical and galactic maps
        //    // open at the same time (with duplicated buttons between the two).
        //    if (button.FindForm() == Form.ActiveForm)
        //    {
        //        isSpaceMasterEnabled = !isSpaceMasterEnabled;
        //    }

        //    ApplySpaceMasterButtonStyle(button);
        //    button.BackgroundImageChanged += OnSpaceMasterButtonBackgroundImageChanged;
        //}

        //private static void ApplySpaceMasterButtonStyle(Button button)
        //{
        //    Bitmap image = isSpaceMasterEnabled ? Resources.Icon_SpaceMasterOn : Resources.Icon_SpaceMasterOff;
        //    Color color = isSpaceMasterEnabled ? enabledSpaceMasterButtonColor : mainTextColor;

        //    button.BackgroundImage = ColorizeImage(image, color);

        //    foreach (var form in lib.GetOpenForms())
        //    {
        //        var buttonCopy = form.Controls.Find(button.Name, true).FirstOrDefault();
        //        if (buttonCopy != null && buttonCopy != button)
        //        {
        //            buttonCopy.BackgroundImageChanged -= OnSpaceMasterButtonBackgroundImageChanged;
        //            buttonCopy.BackgroundImage = button.BackgroundImage;
        //            buttonCopy.BackgroundImageChanged += OnSpaceMasterButtonBackgroundImageChanged;
        //        }
        //    }
        //}

        //private static void OnAutoTurnsButtonBackgroundImageChanged(Object sender, EventArgs e)
        //{
        //    var button = sender as Button;

        //    button.BackgroundImageChanged -= OnAutoTurnsButtonBackgroundImageChanged;

        //    if (button.FindForm() == Form.ActiveForm)
        //    {
        //        isAutoTurnsEnabled = !isAutoTurnsEnabled;
        //    }

        //    ApplyAutoTurnsButtonStyle(button);
        //    button.BackgroundImageChanged += OnAutoTurnsButtonBackgroundImageChanged;
        //}

        //private static void ApplyAutoTurnsButtonStyle(Button button)
        //{
        //    Bitmap image = isAutoTurnsEnabled ? Resources.Icon_AutoTurnsOn : Resources.Icon_AutoTurnsOff;
        //    Color color = isAutoTurnsEnabled ? enabledAutoTurnsButtonColor : mainTextColor;

        //    button.BackgroundImage = ColorizeImage(image, color);

        //    foreach (var form in lib.GetOpenForms())
        //    {
        //        var buttonCopy = form.Controls.Find(button.Name, true).FirstOrDefault();
        //        if (buttonCopy != null && buttonCopy != button)
        //        {
        //            buttonCopy.BackgroundImageChanged -= OnAutoTurnsButtonBackgroundImageChanged;
        //            buttonCopy.BackgroundImage = button.BackgroundImage;
        //            buttonCopy.BackgroundImageChanged += OnAutoTurnsButtonBackgroundImageChanged;
        //        }
        //    }
        //}

        //private static void ChangeButtonStyle(AuroraButton button, Bitmap image, Color textColor, Color? backgroundColor = null)
        //{
        //    Bitmap colorizedImage = ColorizeImage(image, textColor);

        //    ThemeCreator.ThemeCreator.AddImageChange(button, colorizedImage);

        //    if (backgroundColor != null)
        //    {
        //        ThemeCreator.ThemeCreator.AddColorChange(
        //            (Control control) =>
        //            {
        //                return control.GetType() == typeof(Button) && control.Name == lib.KnowledgeBase.GetButtonName(button);
        //            },
        //            new ThemeCreator.ColorChange { BackgroundColor = backgroundColor }
        //        );
        //    }
        //}

        //private static Bitmap ColorizeImage(Bitmap image, Color color)
        //{
        //    var imageAttributes = new ImageAttributes();

        //    float r = color.R / 255f;
        //    float g = color.G / 255f;
        //    float b = color.B / 255f;

        //    float[][] colorMatrixElements = {
        //       new float[] {0, 0, 0, 0, 0},
        //       new float[] {0, 0, 0, 0, 0},
        //       new float[] {0, 0, 0, 0, 0},
        //       new float[] {0, 0, 0, 1, 0},
        //       new float[] {r, g, b, 0, 1}
        //    };

        //    var colorMatrix = new ColorMatrix(colorMatrixElements);

        //    imageAttributes.SetColorMatrix(colorMatrix);

        //    var colorizedImage = new Bitmap(image.Width, image.Height);
        //    var graphics = Graphics.FromImage(colorizedImage);
        //    var rect = new Rectangle(0, 0, image.Width, image.Height);

        //    graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);

        //    return colorizedImage;
        //}


        //var treeNodeConstructorPostfix = new HarmonyMethod(GetType().GetMethod("TreeNodePostfix", AccessTools.all));
        //foreach (var ctor in typeof(TreeNode).GetConstructors())
        //{
        //    harmony.Patch(ctor, postfix: treeNodeConstructorPostfix);
        //}
        
        //private static void TreeNodePostfix(ref TreeNode __instance)
        //{
        //    __instance.ForeColor = GetReplacementColor(__instance.ForeColor);
        //}


        //private static void ApplyListBoxChanges(ListBox listBox)
        //{
        //    if (listBox.BorderStyle == BorderStyle.Fixed3D)
        //    {
        //        listBox.BorderStyle = BorderStyle.FixedSingle;
        //    }
        //}

        //private static void ApplyFlowLayoutPanelChanges(FlowLayoutPanel flowLayoutPanel)
        //{
        //    if (flowLayoutPanel.BorderStyle == BorderStyle.Fixed3D)
        //    {
        //        flowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;
        //    }
        //}

        //private static void ApplyTextBoxChanges(TextBox textBox)
        //{
        //    if (textBox.BorderStyle == BorderStyle.Fixed3D)
        //    {
        //        textBox.BorderStyle = BorderStyle.FixedSingle;
        //    }

        //    // Minor tweak for consistency - align TextBox horizontal padding to match
        //    // ListView and other controls.
        //    SetTextBoxHorizontalPadding(textBox, 4);
        //}

        //private static void ApplyLabelChanges(Label label)
        //{
        //    // Fix mass driver label overflow on top of com bo box issue
        //    if (label.Name == "label17" && label.Text == "Mass Driver Destination")
        //    {
        //        label.Location = new Point(label.Location.X - 10, label.Location.Y);
        //    }
        //}


        //private static void ApplyTabControlChanges(TabControl tabControl)
        //{
        //    tabControl.SizeMode = TabSizeMode.FillToRight;

        //    // Patch tactical map tabs to fit on two lines (necessary due to custom font)
        //    if (tabControl.Name == "tabSidebar")
        //    {
        //        tabControl.Padding = new Point(5, 3);
        //    }
        //}



        //private static void ApplyChanges(Control control)
        //{
            //    if (control.GetType() == typeof(TabControl))
            //    {
            //        ApplyTabControlChanges(control as TabControl);
            //    }
  

            //    else if (control.GetType() == typeof(TreeView))
            //    {
            //        ApplyTreeViewChanges(control as TreeView);
            //    }
   
            //    else if (control.GetType() == typeof(ListBox))
            //    {
            //        ApplyListBoxChanges(control as ListBox);
            //    }
            //    else if (control.GetType() == typeof(FlowLayoutPanel))
            //    {
            //        ApplyFlowLayoutPanelChanges(control as FlowLayoutPanel);
            //    }
            //    else if (control.GetType() == typeof(Label))
            //    {
            //        ApplyLabelChanges(control as Label);
            //    }
            //    else if (control.GetType() == typeof(TextBox))
            //    {
            //        ApplyTextBoxChanges(control as TextBox);
            //    }

        // }

        

        //private static void ApplyTreeViewChanges(TreeView treeView)
        //{
        //    if (treeView.BorderStyle == BorderStyle.Fixed3D)
        //    {
        //        treeView.BorderStyle = BorderStyle.FixedSingle;
        //    }
        //}

        
        //private static bool IsSpaceMasterButton(Button button)
        //{
        //    return button.Name == lib.KnowledgeBase.GetButtonName(AuroraButton.SM);
        //}

        //private static bool IsAutoTurnsButton(Button button)
        //{
        //    return button.Name == lib.KnowledgeBase.GetButtonName(AuroraButton.ToolbarAuto);
        //}

            //private static bool isSpaceMasterEnabled = false;
    //private static bool isAutoTurnsEnabled = false;

    //private const int EM_SETMARGINS = 0xd3;
    //private const int EC_RIGHTMARGIN = 2;
    //private const int EC_LEFTMARGIN = 1;

    //[DllImport("User32.dll")]
    //private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    //private static void SetTextBoxHorizontalPadding(TextBox textBox, int padding)
    //{
    //    SendMessage(textBox.Handle, EM_SETMARGINS, EC_RIGHTMARGIN, padding << 16);
    //    SendMessage(textBox.Handle, EM_SETMARGINS, EC_LEFTMARGIN, padding);
    //}


            // Buttons
            //ThemeCreator.ThemeCreator.AddColorChange(
            //    typeof(Button),
            //    new ThemeCreator.ColorChange { BackgroundColor = buttonBackgroundColor }
            //);

            //lastActiveTimeIncrement = lib.KnowledgeBase.GetButtonName(AuroraButton.Increment);
            //activeSubPulse = lib.KnowledgeBase.GetButtonName(AuroraButton.SubPulse);

            ////ThemeCreator.ThemeCreator.AddFontChange(mainFont);
            ////ThemeCreator.ThemeCreator.AddFontChange(typeof(Button), buttonFont);

            //// Use slightly different text box font size for better alignment and to fix some
            //// overflow issues in System view form ("Specify Minerals" feature in SM mode).
            ////ThemeCreator.ThemeCreator.AddFontChange((Control control) =>
            ////{
            ////    return control.GetType() == typeof(TextBox) && !((TextBox)control).Multiline;
            ////}, singleLineTextBoxFont);

            ////ThemeCreator.ThemeCreator.SetCometTailColor(orbitColor);
            ////ThemeCreator.ThemeCreator.SetPlanetColor(planetColor);

            //ThemeCreator.ThemeCreator.DrawEllipsePrefixAction((graphics, pen) =>
            //{
            //    graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //    // Note that the same color circles are used to mark colonies as well, not just orbits
            //    //if (pen.Color == oldOrbitColor)
            //    //{
            //    //    pen.Color = orbitColor;
            //    //}
            //});

            //ThemeCreator.ThemeCreator.FillEllipsePrefixAction((graphics, brush) =>
            //{
            //    graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //    //if (brush.GetType() == typeof(SolidBrush))
            //    //{
            //    //    var solidBrush = brush as SolidBrush;

            //    //    // This is being overriden by global color contructor hook, but we want to keep
            //    //    // the old yellow color for player contacts, so restore.
            //    //    if (solidBrush.Color == mainTextColor)
            //    //    {
            //    //        solidBrush.Color = oldPlayerContactColor;
            //    //    }
            //    //}
            //});

            //ThemeCreator.ThemeCreator.DrawLinePrefixAction((graphics, pen) =>
            //{
            //    graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //    //// Movement tails
            //    //if (pen.Color == oldCivilianContactColor || pen.Color == oldHostileContactColor
            //    //    || pen.Color == mainTextColor || pen.Color == oldNeutralContactColor)
            //    //{
            //    //    // Restore player contact movement tail color to the old yellow one (was overriden
            //    //    // by global color contructor hook).
            //    //    var newColor = pen.Color == mainTextColor ? oldPlayerContactColor : pen.Color;

            //    //    pen.Color = ControlPaint.Dark(newColor, 0.5f);
            //    //}
            //    //// Comet path (distance ruler also uses the same color but has pen.Width > 1)
            //    //else if (pen.Color == oldCometPathColor && pen.Width == 1)
            //    //{
            //    //    pen.Color = orbitColor;
            //    //}
            //});



            //// Toolbar button images
            ////ChangeButtonStyle(AuroraButton.ZoomIn, Resources.Icon_ZoomIn, mainTextColor);
            ////ChangeButtonStyle(AuroraButton.ToolbarColony, Resources.Icon_Colony, mainTextColor, economicsButtonBackgroundColor);


            //ThemeCreator.ThemeCreator.DrawLinePrefixAction((graphics, pen) =>
            //{
                //// Movement tails
                //if (pen.Color == oldCivilianContactColor || pen.Color == oldHostileContactColor
                //    || pen.Color == mainTextColor || pen.Color == oldNeutralContactColor)
                //{
                //    // Restore player contact movement tail color to the old yellow one (was overriden
                //    // by global color contructor hook).
                //    var newColor = pen.Color == mainTextColor ? oldPlayerContactColor : pen.Color;

                //    pen.Color = ControlPaint.Dark(newColor, 0.5f);
                //}
                //// Comet path (distance ruler also uses the same color but has pen.Width > 1)
                //else if (pen.Color == oldCometPathColor && pen.Width == 1)
                //{
                //    pen.Color = orbitColor;
                //}
