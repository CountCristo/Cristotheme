using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Lib
{
    public enum AuroraType
    {
        TacticalMapForm, EconomicsForm, GameState, ClassDesignForm, CreateProjectForm, FleetWindowForm,
        MissileDesignForm, TurretDesignForm, GroundUnitDesignForm, CommandersWindowForm, MedalsForm,
        RaceWindowForm, SystemViewForm, GalacticMapForm, RaceComparisonForm, DiplomacyForm, TechnologyViewForm,
        MineralsForm, SectorsForm, EventsForm, GameDetailsForm
    }

    // TODO: Currently only contains TacticalMap and GalacticMap buttons.
    public enum AuroraButton
    {
        // Tactical Map
        NormalWaypoint, LastClickedWaypoint, RendezvousWaypoint, PointOfInterestWaypoint, UrgentPointOfInterestWaypoint,
        DeleteWaypoint, ResetWindows, AddCommanderTheme, AddNameTheme, SubPulse, SubPulse5S, SubPulse30S, SubPulse2M,
        SubPulse5M, SubPulse20M, SubPulse1H, SubPulse3H, SubPulse8H, SubPulse1D, SubPulse5D, SubPulse30D, Increment,
        Increment5S, Increment30S, Increment2M, Increment5M, Increment20M, Increment1H, Increment3H, Increment8H,
        Increment1D, Increment5D, Increment30D, ZoomIn, ZoomOut, Up, Left, Right, Down, ToolbarColony, ToolbarIndustry,
        ToolbarMining, ToolbarResearch, ToolbarWealth, ToolbarClass, ToolbarProject, ToolbarFleet, ToolbarMissileDesign,
        ToolbarTurrent, ToolbarGroundForces, ToolbarCommanders, ToolbarMedals, ToolbarRace, ToolbarSystem, ToolbarGalactic,
        ToolbarComparison, ToolbarIntelligence, ToolbarTechnology, ToolbarSurvey, ToolbarSector, ToolbarEvents,
        ToolbarRefreshTactical, ToolbarSave, ToolbarGame, ToolbarAuto, SM,
        // Galactic Map
        ToolbarHabitable, ToolbarRefreshGalactic, ToolbarGrid, ToolbarUndo, ToolbarSavePositions, AwardMedal, CopyLabel,
        UpdateText, LabelFont, NewLabel, DeleteLabel, Hull, Station,
    }

    public class KnowledgeBase
    {
        private readonly Lib Lib;

        internal KnowledgeBase(Lib lib)
        {
            Lib = lib;
        }

        /// <summary>
        /// Dictionary of all known buttons.
        /// TODO: List is incomplete - only contains TacticalMap and GalacticMap buttons.
        /// </summary>
        private Dictionary<AuroraButton, string> AuroraButtons = new Dictionary<AuroraButton, string>
        {
            // Tactical Map
            { AuroraButton.NormalWaypoint, "cmdNormalWP" },
            { AuroraButton.LastClickedWaypoint, "cmdLastClickedWP" },
            { AuroraButton.RendezvousWaypoint,"cmdRendezvousWP" },
            { AuroraButton.PointOfInterestWaypoint,"cmdPOIWP" },
            { AuroraButton.UrgentPointOfInterestWaypoint,"cmdUrgentPOIWP" },
            { AuroraButton.DeleteWaypoint,"cmdDeleteWP" },
            { AuroraButton.ResetWindows,"cmdResetWindows" },
            { AuroraButton.AddCommanderTheme,"cmdAddCmdrTheme" },
            { AuroraButton.AddNameTheme,"cmdAddNameTheme" },
            { AuroraButton.SubPulse,"cmdSubPulse" },
            { AuroraButton.SubPulse5S,"cmdSubPulse5S" },
            { AuroraButton.SubPulse30S,"cmdSubPulse30S" },
            { AuroraButton.SubPulse2M,"cmdSubPulse2M" },
            { AuroraButton.SubPulse5M,"cmdSubPulse5M" },
            { AuroraButton.SubPulse20M,"cmdSubPulse20M" },
            { AuroraButton.SubPulse1H,"cmdSubPulse1H" },
            { AuroraButton.SubPulse3H,"cmdSubPulse3H" },
            { AuroraButton.SubPulse8H,"cmdSubPulse8H" },
            { AuroraButton.SubPulse1D,"cmdSubPulse1D" },
            { AuroraButton.SubPulse5D,"cmdSubPulse5D" },
            { AuroraButton.SubPulse30D,"cmdSubPulse30D" },
            { AuroraButton.Increment,"cmdIncrement" },
            { AuroraButton.Increment5S,"cmdIncrement5S" },
            { AuroraButton.Increment30S,"cmdIncrement30S" },
            { AuroraButton.Increment2M,"cmdIncrement2M" },
            { AuroraButton.Increment5M,"cmdIncrement5M" },
            { AuroraButton.Increment20M,"cmdIncrement20M" },
            { AuroraButton.Increment1H,"cmdIncrement1H" },
            { AuroraButton.Increment3H,"cmdIncrement3H" },
            { AuroraButton.Increment8H,"cmdIncrement8H" },
            { AuroraButton.Increment1D,"cmdIncrement1D" },
            { AuroraButton.Increment5D,"cmdIncrement5D" },
            { AuroraButton.Increment30D,"cmdIncrement30D" },
            { AuroraButton.ZoomIn,"cmdZoomIn" },
            { AuroraButton.ZoomOut,"cmdZoomOut" },
            { AuroraButton.Up,"cmdUp" },
            { AuroraButton.Down,"cmdDown" },
            { AuroraButton.Left,"cmdLeft" },
            { AuroraButton.Right,"cmdRight" },
            { AuroraButton.ToolbarColony, "cmdToolbarColony" },
            { AuroraButton.ToolbarIndustry, "cmdToolbarIndustry" },
            { AuroraButton.ToolbarMining, "cmdToolbarMining" },
            { AuroraButton.ToolbarResearch, "cmdToolbarResearch" },
            { AuroraButton.ToolbarWealth, "cmdToolbarWealth" },
            { AuroraButton.ToolbarClass, "cmdToolbarClass" },
            { AuroraButton.ToolbarProject, "cmdToolbarProject" },
            { AuroraButton.ToolbarFleet, "cmdToolbarFleet" },
            { AuroraButton.ToolbarMissileDesign, "cmdToolbarMissileDesign" },
            { AuroraButton.ToolbarTurrent, "cmdToolbarTurret" },
            { AuroraButton.ToolbarGroundForces, "cmdToolbarGroundForces" },
            { AuroraButton.ToolbarCommanders, "cmdToolbarCommanders" },
            { AuroraButton.ToolbarMedals, "cmdToolbarMedals" },
            { AuroraButton.ToolbarRace, "cmdToolbarRace" },
            { AuroraButton.ToolbarSystem, "cmdToolbarSystem" },
            { AuroraButton.ToolbarGalactic, "cmdToolbarGalactic" },
            { AuroraButton.ToolbarComparison, "cmdToolbarComparison" },
            { AuroraButton.ToolbarIntelligence, "cmdToolbarIntelligence" },
            { AuroraButton.ToolbarTechnology, "cmdToolbarTechnology" },
            { AuroraButton.ToolbarSurvey, "cmdToolbarSurvey" },
            { AuroraButton.ToolbarSector, "cmdToolbarSector" },
            { AuroraButton.ToolbarEvents, "cmdToolbarEvents" },
            { AuroraButton.ToolbarRefreshTactical, "cmdToolbarRefreshTactical" },
            { AuroraButton.ToolbarSave, "cmdToolbarSave" },
            { AuroraButton.ToolbarGame, "cmdToolbarGame" },
            { AuroraButton.ToolbarAuto, "cmdToolbarAuto" },
            { AuroraButton.SM, "cmdSM" },
            // Galactic Map
            { AuroraButton.ToolbarHabitable, "cmdToolbarHabitable" },
            { AuroraButton.ToolbarRefreshGalactic, "cmdToolbarRefreshGalactic" },
            { AuroraButton.ToolbarGrid, "cmdToolbarGrid" },
            { AuroraButton.ToolbarUndo, "cmdToolbarUndo" },
            { AuroraButton.ToolbarSavePositions, "cmdToolbarSavePositions" },
            { AuroraButton.AwardMedal, "cmdAwardMedal" },
            { AuroraButton.CopyLabel, "cmdCopyLabel" },
            { AuroraButton.UpdateText, "cmdUpdateText" },
            { AuroraButton.LabelFont, "cmdLabelFont" },
            { AuroraButton.NewLabel, "cmdNewLabel" },
            { AuroraButton.DeleteLabel, "cmdDeleteLabel" },
            { AuroraButton.Hull, "cmdHull" },
            { AuroraButton.Station, "cmdStation" },
        };

        /// <summary>
        /// Wrapper around AuroraButtons dictionary so that we can easily fix all patches
        /// that rely on Lib if the button names change in the future.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public string GetButtonName(AuroraButton button)
        {
            string buttonName;
            AuroraButtons.TryGetValue(button, out buttonName);
            return buttonName;
        }

        /// <summary>
        /// Wrapper around AuroraButtons dictionary so that we can return the appropriate
        /// checksum version if button names change in the future.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<AuroraButton, string>> GetKnownButtonNames()
        {
            return AuroraButtons.AsEnumerable();
        }

        public IEnumerable<KeyValuePair<AuroraType, string>> GetKnownTypeNames()
        {
            if (Lib.AuroraChecksum == "chm1c7")
            {
                yield return new KeyValuePair<AuroraType, string>(AuroraType.TacticalMapForm, "jt");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.EconomicsForm, "gz");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.GameState, "aw");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.ClassDesignForm, "a8");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.CreateProjectForm, "a2");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.FleetWindowForm, "fs");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.MissileDesignForm, "g2");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.TurretDesignForm, "j2");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.GroundUnitDesignForm, "gg");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.CommandersWindowForm, "az");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.MedalsForm, "a4");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.RaceWindowForm, "hw");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.SystemViewForm, "js");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.GalacticMapForm, "a6");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.RaceComparisonForm, "hu");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.DiplomacyForm, "a0");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.TechnologyViewForm, "j1");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.MineralsForm, "g1");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.SectorsForm, "a5");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.EventsForm, "ff");
                yield return new KeyValuePair<AuroraType, string>(AuroraType.GameDetailsForm, "i2");
            }
        }

        public object GetGameState(Form map)
        {
            var type = Lib.SignatureManager.Get(AuroraType.GameState);
            if (type == null)
            {
                return null;
            }

            try
            {
                foreach (var field in map.GetType().GetFields(AccessTools.all))
                {
                    if (field.FieldType.Name != type.Name)
                    {
                        continue;
                    }
                    Lib.LogDebug($"GameState field {field.Name}");

                    return field.GetValue(map);
                }
            }
            catch (Exception e)
            {
                Lib.LogError($"KnowledgeBase failed to retrieve GameState. {e}");
            }
            

            return null;
        }

        public List<MethodInfo> GetSaveMethods()
        {
            var methods = new List<MethodInfo>();

            var type = Lib.SignatureManager.Get(AuroraType.GameState);
            if (type == null)
            {
                return methods;
            }

            try
            {
                foreach (var method in type.GetMethods(AccessTools.all))
                {
                    var parameters = method.GetParameters();

                    if (parameters.Length != 1)
                    {
                        continue;
                    }

                    if (parameters[0].ParameterType.Name != "SQLiteConnection")
                    {
                        continue;
                    }

                    methods.Add(method);
                }
            }
            catch (Exception e)
            {
                Lib.LogError($"KnowledgeBase failed to retrieve save methods. {e}");
            }

            return methods;
        }

        public IEnumerable<Button> GetTimeIncrementButtons()
        {
            if (Lib.TacticalMap == null) return new List<Button>();
            return UIManager.IterateControls(Lib.TacticalMap)
                .Where(c => c.GetType() == typeof(Button) && ((Button)c).Name.Contains("cmdIncrement"))
                .Select(c => (Button)c);
        }

        public IEnumerable<Button> GetTimeIncrementButtonsGalacticMap()
        {
            var galacticMap = Lib.GetOpenForms().FirstOrDefault(f => f.Name == "GalacticMap");
            if (galacticMap == null) return Enumerable.Empty<Button>();
            return UIManager.IterateControls(galacticMap)
                .Where(c => c.GetType() == typeof(Button) && ((Button)c).Name.Contains("cmdIncrement"))
                .Select(c => (Button)c);
        }

        public IEnumerable<Button> GetSubPulseButtons()
        {
            if (Lib.TacticalMap == null) return new List<Button>();
            return UIManager.IterateControls(Lib.TacticalMap)
                .Where(c => c.GetType() == typeof(Button) && ((Button)c).Name.Contains("cmdSubPulse"))
                .Select(c => (Button)c);
        }

        public string GetFormOpenButtonName(AuroraType type)
        {
            switch (type)
            {
                case AuroraType.EconomicsForm: return GetButtonName(AuroraButton.ToolbarColony);
                case AuroraType.ClassDesignForm: return GetButtonName(AuroraButton.ToolbarClass);
                case AuroraType.CreateProjectForm: return GetButtonName(AuroraButton.ToolbarProject);
                case AuroraType.FleetWindowForm: return GetButtonName(AuroraButton.ToolbarFleet);
                case AuroraType.MissileDesignForm: return GetButtonName(AuroraButton.ToolbarMissileDesign);
                case AuroraType.TurretDesignForm: return GetButtonName(AuroraButton.ToolbarTurrent);
                case AuroraType.GroundUnitDesignForm: return GetButtonName(AuroraButton.ToolbarGroundForces);
                case AuroraType.CommandersWindowForm: return GetButtonName(AuroraButton.ToolbarCommanders);
                case AuroraType.MedalsForm: return GetButtonName(AuroraButton.ToolbarMedals);
                case AuroraType.RaceWindowForm: return GetButtonName(AuroraButton.ToolbarRace);
                case AuroraType.SystemViewForm: return GetButtonName(AuroraButton.ToolbarSystem);
                case AuroraType.GalacticMapForm: return GetButtonName(AuroraButton.ToolbarGalactic);
                case AuroraType.RaceComparisonForm: return GetButtonName(AuroraButton.ToolbarComparison);
                case AuroraType.DiplomacyForm: return GetButtonName(AuroraButton.ToolbarIntelligence);
                case AuroraType.TechnologyViewForm: return GetButtonName(AuroraButton.ToolbarTechnology);
                case AuroraType.MineralsForm: return GetButtonName(AuroraButton.ToolbarSurvey);
                case AuroraType.SectorsForm: return GetButtonName(AuroraButton.ToolbarSector);
                case AuroraType.EventsForm: return GetButtonName(AuroraButton.ToolbarEvents);
                case AuroraType.GameDetailsForm: return GetButtonName(AuroraButton.ToolbarGame);
                default: return null;
            }
        }
    }
}
