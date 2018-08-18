using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using SolutionManager.Models;
using SolutionManager.Manager;

namespace SolutionManager
{
    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            base.ExecuteMethod(LoadSolutions);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);
            //mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
            LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
        }

        #region Events

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void tsbReloadSolutions_Click(object sender, EventArgs e)
        {
            this.LoadSolutions();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            this.DeleteSolutions();
        }
        
        private void tsbCreateSolutions_Click(object sender, EventArgs e)
        {
            this.CreateSolutions();
        }

        #endregion Events


        #region Private Methods

        private void CreateSolutions()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Creating solutions...",
                Work = (worker, args) =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            Entity creator = new Entity("solution");
                            creator["uniquename"] = string.Format("Solution{0}", i);
                            creator["friendlyname"] = string.Format("Solution{0}", i);
                            creator["version"] = "1.0";
                            creator["description"] = string.Format("Description for solution {0}", i);
                            creator["publisherid"] = new EntityReference("publisher", Guid.Parse("00000001-0000-0000-0000-00000000005a"));

                            base.Service.Create(creator);
                        }
                        catch(Exception ex)
                        { }
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    this.LoadSolutions();
                }
            });
        }

        private void LoadSolutions()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading solutions...",
                Work = (worker, args) =>
                {
                    QueryExpression qry = new QueryExpression("solution");
                    qry.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                    qry.ColumnSet = new ColumnSet("description", "friendlyname", "uniquename", "version");
                    qry.Orders.Add(new OrderExpression("friendlyname", OrderType.Ascending));

                    EntityCollection results = base.Service.RetrieveMultiple(qry);

                    MySolutionManager solutionManager = new MySolutionManager();

                    List<Solution> solutions = new List<Solution>();

                    results.Entities.ToList().ForEach(e => solutions.Add(solutionManager.ConvertToSolution(e)));

                    args.Result = solutions;
                },
                PostWorkCallBack = (args) =>
                {
                    List<Solution> solutions = (List<Solution>)args.Result;
                    dgSolutions.DataSource = solutions;

                    //Add checkbox columns (if not exists)
                    DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn();
                    chk.Name = "chk";
                    chk.HeaderText = string.Empty;
                    if (!dgSolutions.Columns.Contains("chk")) dgSolutions.Columns.Insert(0, chk);

                    //Column layout
                    dgSolutions.Columns["EntityName"].Visible = false;
                    dgSolutions.Columns["SolutionId"].Visible = false;
                    dgSolutions.Columns["chk"].Width = 25;
                    dgSolutions.Columns["Version"].Width = (dgSolutions.Width * 10 / 100);
                    dgSolutions.Columns["Version"].ReadOnly = true;
                    dgSolutions.Columns["UniqueName"].Width = (dgSolutions.Width * 20 / 100);
                    dgSolutions.Columns["UniqueName"].ReadOnly = true;
                    dgSolutions.Columns["FriendlyName"].Width = (dgSolutions.Width * 20 / 100);
                    dgSolutions.Columns["FriendlyName"].ReadOnly = true;
                    dgSolutions.Columns["Description"].Width = (dgSolutions.Width * 40 / 100);
                    dgSolutions.Columns["Description"].ReadOnly = true;
                }
            });

        }

        private void DeleteSolutions()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Deleting solutions...",
                Work = (worker, args) =>
                {
                    dgSolutions.CommitEdit(DataGridViewDataErrorContexts.Commit);

                    //Get the checked Configurations
                    List<Solution> solutionsToDelete = new List<Solution>();

                    foreach (DataGridViewRow row in dgSolutions.Rows)
                    {
                        if (Convert.ToBoolean(row.Cells["chk"].Value) == true)
                        {
                            Solution solution = (Solution)row.DataBoundItem;
                            solutionsToDelete.Add(solution);
                        }
                    }

                    if (solutionsToDelete.Count == 0)
                    {
                        MessageBox.Show("Please select at least one solution to delete", "Select solutions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    solutionsToDelete.ForEach(s => base.Service.Delete(s.EntityName, s.SolutionId));

                },
                PostWorkCallBack = (args) =>
                {
                    this.LoadSolutions();
                }
            });
        }


        #endregion private Methods

    }
}