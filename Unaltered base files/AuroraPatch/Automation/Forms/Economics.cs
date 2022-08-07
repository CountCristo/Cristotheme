using Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Automation.Forms
{
    public class Economics : AuroraForm
    {
        public void CreateConstructionProject(string population, string project_type, string project_name, int amount, int percentage)
        {
            var action = new Action<Form>(form =>
            {
                population += " ";

                var populations = GetPopulations(form);
                foreach (var pop in populations)
                {
                    if (pop.Text.StartsWith(population))
                    {
                        SelectPopulation(form, pop);
                        SelectTab(form, "tabIndustry");

                        var constructiontypecombo = UIManager.GetControlByName<ComboBox>(form, "cboConstructionType");
                        foreach (var construction_type in constructiontypecombo.Items)
                        {
                            if (construction_type.ToString() == project_type)
                            {
                                constructiontypecombo.SelectedItem = construction_type;

                                var projects = UIManager.GetControlByName<ListBox>(form, "lstPI");

                                foreach (var construction_project in projects.Items.OfType<object>().ToList())
                                {
                                    var displayed = construction_project.GetType().GetProperty(projects.DisplayMember).GetValue(construction_project, null).ToString();

                                    if (displayed == project_name)
                                    {
                                        projects.SelectedItem = construction_project;

                                        var items = UIManager.GetControlByName<TextBox>(form, "txtItems");
                                        items.Text = amount.ToString();

                                        var percentages = UIManager.GetControlByName<TextBox>(form, "txtPercentage");
                                        percentages.Text = percentage.ToString();

                                        var create = UIManager.GetControlByName<Button>(form, "cmdCreate");
                                        create.PerformClick();

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            });

            Automation.Lib.UIManager.RunOnForm(Lib.AuroraType.EconomicsForm, action);
        }

        public List<TreeNode> GetPopulations(Form form)
        {
            var populations = new List<TreeNode>();

            var poptree = UIManager.GetControlByName<TreeView>(form, "tvPopList");
            foreach (TreeNode system in poptree.Nodes)
            {
                foreach (TreeNode pop in system.Nodes)
                {
                    populations.Add(pop);
                }
            }

            return populations;
        }

        public void SelectPopulation(Form form, TreeNode population)
        {
            var poptree = UIManager.GetControlByName<TreeView>(form, "tvPopList");
            poptree.SelectedNode = population;
        }

        public void SelectTab(Form form, string name)
        {
            var tabcontrol = UIManager.GetControlByName<TabControl>(form, "tabPopulation");
            var tabpage = UIManager.GetControlByName<TabPage>(form, name);
            tabcontrol.SelectedTab = tabpage;
        }
    }
}
