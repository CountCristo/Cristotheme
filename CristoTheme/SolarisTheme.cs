using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using HarmonyLib;
using Lib;
using SolarisTheme.Properties;

namespace SolarisTheme
{
    public class SolarisTheme : AuroraPatch.Patch
    {
        public override string Description => "Solaris Theme";
        public override IEnumerable<string> Dependencies => new[] { "ThemeCreator", "Lib" };

        private static Lib.Lib lib;

        // Fonts
        private static readonly FontFamily fontFamily = new FontFamily("Tahoma");
        private static readonly Font mainFont = new Font(fontFamily, 8.25f);
        private static readonly Font singleLineTextBoxFont = new Font(fontFamily, 8);
        private static readonly Font buttonFont = new Font(fontFamily, 7, FontStyle.Bold);

        // Our new colors
        private static readonly Color mainBackgroundColor = Color.FromArgb(12, 12, 12);
        private static readonly Color mainTextColor = Color.FromArgb(210, 210, 210);
        private static readonly Color disabledTextColor = ControlPaint.Dark(mainTextColor, 0.1f);
        private static readonly Color buttonBackgroundColor = Color.FromArgb(23, 26, 39);
        private static readonly Color planetColor = Color.FromArgb(128, 128, 128);
        private static readonly Color orbitColor = Color.FromArgb(128, planetColor);
        private static readonly Color enabledSpaceMasterButtonColor = Color.FromArgb(248, 231, 28);
        private static readonly Color enabledAutoTurnsButtonColor = Color.FromArgb(126, 211, 33);

        // Toolbar button background colors
        private static readonly Color economicsButtonBackgroundColor = Color.FromArgb(26, 45, 46);
        private static readonly Color designButtonBackgroundColor = Color.FromArgb(41, 44, 46);
        private static readonly Color fleetButtonBackgroundColor = Color.FromArgb(45, 26, 26);
        private static readonly Color groundForcesButtonBackgroundColor = Color.FromArgb(42, 45, 28);
        private static readonly Color intelligenceButtonBackgroundColor = Color.FromArgb(47, 38, 47);
        private static readonly Color explorationButtonBackgroundColor = Color.FromArgb(24, 27, 78);
        private static readonly Color personnelButtonBackgroundColor = Color.FromArgb(18, 41, 58);
        private static readonly Color surveyButtonBackgroundColor = Color.FromArgb(40, 31, 24);
        private static readonly Color technologyButtonBackgroundColor = Color.FromArgb(42, 22, 45);
        private static readonly Color sectorButtonBackgroundColor = Color.FromArgb(20, 45, 31);

        // Old colors
        private static readonly Color oldTextColor = Color.FromArgb(255, 255, 192);
        private static readonly Color oldBackgrounColor = Color.FromArgb(0, 0, 64);
        private static readonly Color oldPlayerContactColor = Color.FromArgb(255, 255, 192);
        private static readonly Color oldNeutralContactColor = Color.FromArgb(144, 238, 144);
        private static readonly Color oldCivilianContactColor = Color.FromArgb(0, 206, 209);
        private static readonly Color oldHostileContactColor = Color.FromArgb(255, 0, 0);
        private static readonly Color oldCometPathColor = Color.LimeGreen;
        private static readonly Color oldOrbitColor = Color.LimeGreen;
        private static readonly Color oldDisabledTextColor = Color.LightGray;
        private static readonly Color oldEnabledButtonBackgroundColor = Color.FromArgb(0, 0, 120);

        private static string lastActiveTimeIncrement;
        private static string activeSubPulse;
        private static bool isSpaceMasterEnabled = false;
        private static bool isAutoTurnsEnabled = false;

        private const int EM_SETMARGINS = 0xd3;
        private const int EC_RIGHTMARGIN = 2;
        private const int EC_LEFTMARGIN = 1;

        [DllImport("User32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private static void SetTextBoxHorizontalPadding(TextBox textBox, int padding)
        {
            SendMessage(textBox.Handle, EM_SETMARGINS, EC_RIGHTMARGIN, padding << 16);
            SendMessage(textBox.Handle, EM_SETMARGINS, EC_LEFTMARGIN, padding);
        }

        private static bool IsTimeIncrementButton(Button button)
        {
            return button.Name.StartsWith("cmdIncrement");
        }

        private static bool IsSubPulseButton(Button button)
        {
            return button.Name.StartsWith("cmdSubPulse");
        }

        private static bool IsSpaceMasterButton(Button button)
        {
            return button.Name == lib.KnowledgeBase.GetButtonName(AuroraButton.SM);
        }

        private static bool IsAutoTurnsButton(Button button)
        {
            return button.Name == lib.KnowledgeBase.GetButtonName(AuroraButton.ToolbarAuto);
        }

        protected override void Loaded(Harmony harmony)
        {
            lib = GetDependency<Lib.Lib>("Lib");

            // Buttons
            ThemeCreator.ThemeCreator.AddColorChange(
                typeof(Button),
                new ThemeCreator.ColorChange { BackgroundColor = buttonBackgroundColor }
            );

            lastActiveTimeIncrement = lib.KnowledgeBase.GetButtonName(AuroraButton.Increment);
            activeSubPulse = lib.KnowledgeBase.GetButtonName(AuroraButton.SubPulse);

            ThemeCreator.ThemeCreator.AddFontChange(mainFont);
            ThemeCreator.ThemeCreator.AddFontChange(typeof(Button), buttonFont);

            // Use slightly different text box font size for better alignment and to fix some
            // overflow issues in System view form ("Specify Minerals" feature in SM mode).
            ThemeCreator.ThemeCreator.AddFontChange((Control control) =>
            {
                return control.GetType() == typeof(TextBox) && !((TextBox)control).Multiline;
            }, singleLineTextBoxFont);

            ThemeCreator.ThemeCreator.SetCometTailColor(orbitColor);
            ThemeCreator.ThemeCreator.SetPlanetColor(planetColor);

            ThemeCreator.ThemeCreator.DrawEllipsePrefixAction((graphics, pen) =>
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Note that the same color circles are used to mark colonies as well, not just orbits
                if (pen.Color == oldOrbitColor)
                {
                    pen.Color = orbitColor;
                }
            });

            ThemeCreator.ThemeCreator.FillEllipsePrefixAction((graphics, brush) =>
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                if (brush.GetType() == typeof(SolidBrush))
                {
                    var solidBrush = brush as SolidBrush;

                    // This is being overriden by global color contructor hook, but we want to keep
                    // the old yellow color for player contacts, so restore.
                    if (solidBrush.Color == mainTextColor)
                    {
                        solidBrush.Color = oldPlayerContactColor;
                    }
                }
            });

            ThemeCreator.ThemeCreator.DrawLinePrefixAction((graphics, pen) =>
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Movement tails
                if (pen.Color == oldCivilianContactColor || pen.Color == oldHostileContactColor
                    || pen.Color == mainTextColor || pen.Color == oldNeutralContactColor)
                {
                    // Restore player contact movement tail color to the old yellow one (was overriden
                    // by global color contructor hook).
                    var newColor = pen.Color == mainTextColor ? oldPlayerContactColor : pen.Color;

                    pen.Color = ControlPaint.Dark(newColor, 0.5f);
                }
                // Comet path (distance ruler also uses the same color but has pen.Width > 1)
                else if (pen.Color == oldCometPathColor && pen.Width == 1)
                {
                    pen.Color = orbitColor;
                }
            });

            ThemeCreator.ThemeCreator.DrawStringPrefixAction((graphics, s, font, brush) =>
            {
                if (brush.GetType() == typeof(SolidBrush))
                {
                    var solidBrush = brush as SolidBrush;

                    if (solidBrush.Color == oldPlayerContactColor)
                    {
                        solidBrush.Color = mainTextColor;
                    }
                }
            });

            // Toolbar button images
            ChangeButtonStyle(AuroraButton.ZoomIn, Resources.Icon_ZoomIn, mainTextColor);
            ChangeButtonStyle(AuroraButton.ZoomOut, Resources.Icon_ZoomOut, mainTextColor);
            ChangeButtonStyle(AuroraButton.Up, Resources.Icon_Up, mainTextColor);
            ChangeButtonStyle(AuroraButton.Down, Resources.Icon_Down, mainTextColor);
            ChangeButtonStyle(AuroraButton.Left, Resources.Icon_Left, mainTextColor);
            ChangeButtonStyle(AuroraButton.Right, Resources.Icon_Right, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarColony, Resources.Icon_Colony, mainTextColor, economicsButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarIndustry, Resources.Icon_Industry, mainTextColor, economicsButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarMining, Resources.Icon_Mining, mainTextColor, economicsButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarResearch, Resources.Icon_Research, mainTextColor, economicsButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarWealth, Resources.Icon_Wealth, mainTextColor, economicsButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarClass, Resources.Icon_Class, mainTextColor, designButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarProject, Resources.Icon_Project, mainTextColor, designButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarFleet, Resources.Icon_Fleet, mainTextColor, fleetButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarMissileDesign, Resources.Icon_MissileDesign, mainTextColor, designButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarTurrent, Resources.Icon_Turrent, mainTextColor, designButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarGroundForces, Resources.Icon_GroundForces, mainTextColor, groundForcesButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarCommanders, Resources.Icon_Commanders, mainTextColor, personnelButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarMedals, Resources.Icon_Medals, mainTextColor, personnelButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarRace, Resources.Icon_Race, mainTextColor, intelligenceButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarSystem, Resources.Icon_System, mainTextColor, explorationButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarGalactic, Resources.Icon_Galactic, mainTextColor, explorationButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarHabitable, Resources.Icon_Galactic, mainTextColor, explorationButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarComparison, Resources.Icon_Comparison, mainTextColor, intelligenceButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarIntelligence, Resources.Icon_Intelligence, mainTextColor, intelligenceButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarTechnology, Resources.Icon_Technology, mainTextColor, technologyButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarSurvey, Resources.Icon_Survey, mainTextColor, surveyButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarSector, Resources.Icon_Sector, mainTextColor, sectorButtonBackgroundColor);
            ChangeButtonStyle(AuroraButton.ToolbarEvents, Resources.Icon_Events, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarRefreshTactical, Resources.Icon_Refresh, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarRefreshGalactic, Resources.Icon_Refresh, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarSave, Resources.Icon_Save, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarGame, Resources.Icon_Game, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarGrid, Resources.Icon_Grid, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarUndo, Resources.Icon_Undo, mainTextColor);
            ChangeButtonStyle(AuroraButton.ToolbarSavePositions, Resources.Icon_SavePositions, mainTextColor);

            var colorConstructorPostfix = new HarmonyMethod(GetType().GetMethod("ColorConstructorPostfix", AccessTools.all));

            // Patch all Color.FromArgb overloads for color overrides
            foreach (var method in typeof(Color).GetMethods(AccessTools.all))
            {
                if (method.Name == "FromArgb")
                {
                    harmony.Patch(method, postfix: colorConstructorPostfix);
                }
            }

            // Also hook into some predefined/named color properties
            harmony.Patch(typeof(Color).GetMethod("get_LightGray"), postfix: colorConstructorPostfix);

            // Hook into Aurora forms constructors for some more advanced overrides
            var formConstructorPostfix = new HarmonyMethod(GetType().GetMethod("FormConstructorPostfix", AccessTools.all));

            foreach (var form in AuroraAssembly.GetTypes().Where(t => typeof(Form).IsAssignableFrom(t)))
            {
                foreach (var ctor in form.GetConstructors())
                {
                    harmony.Patch(ctor, postfix: formConstructorPostfix);
                }
            }
        }

        private static void ColorConstructorPostfix(ref Color __result)
        {
            if (__result == oldTextColor)
            {
                __result = mainTextColor;
            }
            else if (__result == oldBackgrounColor || __result == oldEnabledButtonBackgroundColor)
            {
                __result = mainBackgroundColor;
            }
            else if (__result == oldDisabledTextColor)
            {
                __result = disabledTextColor;
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
            if (control.GetType() == typeof(TabControl))
            {
                ApplyTabControlChanges(control as TabControl);
            }
            else if (control.GetType() == typeof(Button))
            {
                ApplyButtonChanges(control as Button);
            }
            else if (control.GetType() == typeof(ComboBox))
            {
                ApplyComboBoxChanges(control as ComboBox);
            }
            else if (control.GetType() == typeof(TreeView))
            {
                ApplyTreeViewChanges(control as TreeView);
            }
            else if (control.GetType() == typeof(ListView))
            {
                ApplyListViewChanges(control as ListView);
            }
            else if (control.GetType() == typeof(ListBox))
            {
                ApplyListBoxChanges(control as ListBox);
            }
            else if (control.GetType() == typeof(FlowLayoutPanel))
            {
                ApplyFlowLayoutPanelChanges(control as FlowLayoutPanel);
            }
            else if (control.GetType() == typeof(Label))
            {
                ApplyLabelChanges(control as Label);
            }
            else if (control.GetType() == typeof(TextBox))
            {
                ApplyTextBoxChanges(control as TextBox);
            }
            else if (control is Form)
            {
                ApplyFormChanges(control as Form);
            }
        }

        private static void ApplyTabControlChanges(TabControl tabControl)
        {
            tabControl.SizeMode = TabSizeMode.FillToRight;

            // Patch tactical map tabs to fit on two lines (necessary due to custom font)
            if (tabControl.Name == "tabSidebar")
            {
                tabControl.Padding = new Point(5, 3);
            }
        }

        private static void ApplyButtonChanges(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = mainBackgroundColor;
            button.FlatAppearance.BorderSize = 2;

            // With some exceptions just enable auto size for buttons (necessary due to custom font)
            if (button.Name != lib.KnowledgeBase.GetButtonName(AuroraButton.SubPulse)
                && button.Name != lib.KnowledgeBase.GetButtonName(AuroraButton.Increment))
            {
                button.AutoSize = true;
            }

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
            else if (IsSpaceMasterButton(button))
            {
                ApplySpaceMasterButtonStyle(button);
                button.BackgroundImageChanged += OnSpaceMasterButtonBackgroundImageChanged;
            }
            else if (IsAutoTurnsButton(button))
            {
                ApplyAutoTurnsButtonStyle(button);
                button.BackgroundImageChanged += OnAutoTurnsButtonBackgroundImageChanged;
            }
        }

        private static void ApplyComboBoxChanges(ComboBox comboBox)
        {
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private static void ApplyTreeViewChanges(TreeView treeView)
        {
            if (treeView.BorderStyle == BorderStyle.Fixed3D)
            {
                treeView.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private static void ApplyListViewChanges(ListView listView)
        {
            if (listView.BorderStyle == BorderStyle.Fixed3D)
            {
                listView.BorderStyle = BorderStyle.FixedSingle;
            }

            if (listView.View == View.Details && listView.Columns.Count > 1)
            {
                listView.FullRowSelect = true;
            }
        }

        private static void ApplyListBoxChanges(ListBox listBox)
        {
            if (listBox.BorderStyle == BorderStyle.Fixed3D)
            {
                listBox.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private static void ApplyFlowLayoutPanelChanges(FlowLayoutPanel flowLayoutPanel)
        {
            if (flowLayoutPanel.BorderStyle == BorderStyle.Fixed3D)
            {
                flowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private static void ApplyTextBoxChanges(TextBox textBox)
        {
            if (textBox.BorderStyle == BorderStyle.Fixed3D)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }

            // Minor tweak for consistency - align TextBox horizontal padding to match
            // ListView and other controls.
            SetTextBoxHorizontalPadding(textBox, 4);
        }

        private static void ApplyLabelChanges(Label label)
        {
            // Fix mass driver label overflow on top of combo box issue
            if (label.Name == "label17" && label.Text == "Mass Driver Destination")
            {
                label.Location = new Point(label.Location.X - 10, label.Location.Y);
            }
        }

        private static void ApplyFormChanges(Form form)
        {
            form.ShowIcon = false; // Aurora uses default Windows Forms icons
        }

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
            button.FlatAppearance.BorderColor = isActive
                ? ControlPaint.Light(buttonBackgroundColor, 0.5f)
                : mainBackgroundColor;
        }

        private static void OnSpaceMasterButtonBackgroundImageChanged(Object sender, EventArgs e)
        {
            var button = sender as Button;

            button.BackgroundImageChanged -= OnSpaceMasterButtonBackgroundImageChanged;

            // NOTE: This guard is needed as you can have both tactical and galactic maps
            // open at the same time (with duplicated buttons between the two).
            if (button.FindForm() == Form.ActiveForm)
            {
                isSpaceMasterEnabled = !isSpaceMasterEnabled;
            }

            ApplySpaceMasterButtonStyle(button);
            button.BackgroundImageChanged += OnSpaceMasterButtonBackgroundImageChanged;
        }

        private static void ApplySpaceMasterButtonStyle(Button button)
        {
            Bitmap image = isSpaceMasterEnabled ? Resources.Icon_SpaceMasterOn : Resources.Icon_SpaceMasterOff;
            Color color = isSpaceMasterEnabled ? enabledSpaceMasterButtonColor : mainTextColor;

            button.BackgroundImage = ColorizeImage(image, color);

            foreach (var form in lib.GetOpenForms())
            {
                var buttonCopy = form.Controls.Find(button.Name, true).FirstOrDefault();
                if (buttonCopy != null && buttonCopy != button)
                {
                    buttonCopy.BackgroundImageChanged -= OnSpaceMasterButtonBackgroundImageChanged;
                    buttonCopy.BackgroundImage = button.BackgroundImage;
                    buttonCopy.BackgroundImageChanged += OnSpaceMasterButtonBackgroundImageChanged;
                }
            }
        }

        private static void OnAutoTurnsButtonBackgroundImageChanged(Object sender, EventArgs e)
        {
            var button = sender as Button;

            button.BackgroundImageChanged -= OnAutoTurnsButtonBackgroundImageChanged;

            if (button.FindForm() == Form.ActiveForm)
            {
                isAutoTurnsEnabled = !isAutoTurnsEnabled;
            }

            ApplyAutoTurnsButtonStyle(button);
            button.BackgroundImageChanged += OnAutoTurnsButtonBackgroundImageChanged;
        }

        private static void ApplyAutoTurnsButtonStyle(Button button)
        {
            Bitmap image = isAutoTurnsEnabled ? Resources.Icon_AutoTurnsOn : Resources.Icon_AutoTurnsOff;
            Color color = isAutoTurnsEnabled ? enabledAutoTurnsButtonColor : mainTextColor;

            button.BackgroundImage = ColorizeImage(image, color);

            foreach (var form in lib.GetOpenForms())
            {
                var buttonCopy = form.Controls.Find(button.Name, true).FirstOrDefault();
                if (buttonCopy != null && buttonCopy != button)
                {
                    buttonCopy.BackgroundImageChanged -= OnAutoTurnsButtonBackgroundImageChanged;
                    buttonCopy.BackgroundImage = button.BackgroundImage;
                    buttonCopy.BackgroundImageChanged += OnAutoTurnsButtonBackgroundImageChanged;
                }
            }
        }

        private static void ChangeButtonStyle(AuroraButton button, Bitmap image, Color textColor, Color? backgroundColor = null)
        {
            Bitmap colorizedImage = ColorizeImage(image, textColor);

            ThemeCreator.ThemeCreator.AddImageChange(button, colorizedImage);

            if (backgroundColor != null)
            {
                ThemeCreator.ThemeCreator.AddColorChange(
                    (Control control) =>
                    {
                        return control.GetType() == typeof(Button) && control.Name == lib.KnowledgeBase.GetButtonName(button);
                    },
                    new ThemeCreator.ColorChange { BackgroundColor = backgroundColor }
                );
            }
        }

        private static Bitmap ColorizeImage(Bitmap image, Color color)
        {
            var imageAttributes = new ImageAttributes();

            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float[][] colorMatrixElements = {
               new float[] {0, 0, 0, 0, 0},
               new float[] {0, 0, 0, 0, 0},
               new float[] {0, 0, 0, 0, 0},
               new float[] {0, 0, 0, 1, 0},
               new float[] {r, g, b, 0, 1}
            };

            var colorMatrix = new ColorMatrix(colorMatrixElements);

            imageAttributes.SetColorMatrix(colorMatrix);

            var colorizedImage = new Bitmap(image.Width, image.Height);
            var graphics = Graphics.FromImage(colorizedImage);
            var rect = new Rectangle(0, 0, image.Width, image.Height);

            graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);

            return colorizedImage;
        }
    }
}
