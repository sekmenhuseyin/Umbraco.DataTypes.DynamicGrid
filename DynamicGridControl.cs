using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Umbraco.DataTypes.DynamicGrid.Helpers;

namespace Umbraco.DataTypes.DynamicGrid
{
    public class DynamicGridControl : UpdatePanel
    {
        public int NumberOfRows { get; set; }
        public int NumberOfCols { get; set; }

        private string _xmlValue { get; set; }
        public string XmlValue
        {
            get
            {
                string possibleXml = XmlHelpers.DataSetToXMLString(XmlHelpers.TableToDataSet((Table)tablePanel.FindControl("DynamicGridTable")));
                if (XmlHelpers.IsValidXml(possibleXml))
                    return possibleXml;

                return string.Empty;
            }
            set
            {
                if (XmlHelpers.IsValidXml(value))
                    _xmlValue = value;
            }
        }

        // todo: don't want to use this, but how do i add multiple identical controls to the same page, and still read them with "findcontrol"?
        private string _uniqueID { get { return this.UniqueID; } }

        /// =================================================================================
        #region UI & Counters
        /// =================================================================================

        // Buttons
        private LinkButton addColumn;
        private LinkButton removeColumn;
        private LinkButton addRow;
        private LinkButton removeRow;
        private LinkButton resetTable;
        private HtmlGenericControl linksSpacer { get { return new HtmlGenericControl("span") { InnerHtml = "&nbsp;|&nbsp;" }; } }

        //Panel to hold the controls
        private Panel tablePanel;

        /// <summary>
        /// A counter that stores the number of rows to be created.
        /// </summary>
        private int rowCount
        {
            get { return (int)ViewState["rowCount"]; }
            set { ViewState["rowCount"] = value; }
        }

        /// <summary>
        /// A counter that stores the number of columns to be created.
        /// </summary>
        private int colCount
        {
            get { return (int)ViewState["colCount"]; }
            set { ViewState["colCount"] = value; }
        }

        /// =================================================================================
        #endregion


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.UpdateMode = UpdatePanelUpdateMode.Conditional;

            // Initialize buttons & Panel
            addColumn = new LinkButton();
            removeColumn = new LinkButton();
            addRow = new LinkButton();
            removeRow = new LinkButton();
            resetTable = new LinkButton();
            tablePanel = new Panel();
            tablePanel.ID = "PanelPlaceholder";// +_uniqueID;// +UniqueID; // +_uniqueID;// "PanelPlaceHolder_DynamicGridControl";

            addColumn.ID = "addColumn";// + "_" + _uniqueID;// + UniqueID; ;// + _uniqueID;// base.UniqueID;// base.ID;
            removeColumn.ID = "removeColumn";// + "_" + _uniqueID;//+ UniqueID; ;// + _uniqueID;//base.UniqueID;//base.ID;
            addRow.ID = "addRow";// + "_" + _uniqueID;//UniqueID; ;//+ _uniqueID;//base.UniqueID;//base.ID;
            removeRow.ID = "removeRow";//+ "_" + _uniqueID;//UniqueID; ;// + _uniqueID;// base.UniqueID;// base.ID;
            resetTable.ID = "resetTable";// + "_" + _uniqueID;// + UniqueID; ;//+ _uniqueID;// base.UniqueID;// base.ID;

            // Set text
            addColumn.Text = "Add Column";
            removeColumn.Text = "Remove Column";
            addRow.Text = "Add Row";
            removeRow.Text = "Remove Row";
            resetTable.Text = "Reset Table";

            // Add to Update Panel
            tablePanel.Controls.Add(addColumn);
            tablePanel.Controls.Add(linksSpacer);
            tablePanel.Controls.Add(removeColumn);
            tablePanel.Controls.Add(linksSpacer);
            tablePanel.Controls.Add(addRow);
            tablePanel.Controls.Add(linksSpacer);
            tablePanel.Controls.Add(removeRow);
            tablePanel.Controls.Add(linksSpacer);
            tablePanel.Controls.Add(resetTable);

            base.ContentTemplateContainer.Controls.Add(tablePanel);

            // no postback
            // could have values in db, or could prepopulate with defaults
            if (!Page.IsPostBack)
            {
                // get from db
                if (!string.IsNullOrEmpty(_xmlValue))
                {
                    //Create dataset/table from XML string in database
                    DataSet ds = XmlHelpers.XMLStringToDataSet(_xmlValue);
                    //DataSet ds = XmlHelpers.XMLStringToDataSet(HttpContext.Current.Server.HtmlDecode(_xmlValue));
                    Table dimensionsTable = XmlHelpers.DataSetToTable(ds);//, UniqueID);
                    tablePanel.Controls.Add(dimensionsTable);

                    DataTable dt = ds.Tables["Row"];
                    rowCount = dt.Rows.Count;
                    colCount = dt.Columns.Count;
                }
                // prepopulate with defaults
                else
                {
                    rowCount = NumberOfRows;
                    colCount = NumberOfCols;

                    tablePanel.Controls.Add(XmlHelpers.DataSetToTable(XmlHelpers.DefaultDataSet(rowCount, colCount)));//, UniqueID));
                }
            }
            // postback
            else
            {
                // We're using GetPostBackControl instead of the buttons' click events due to Page Cycle issues
                // See http://stackoverflow.com/questions/2800496/c-counter-requires-2-button-clicks-to-update
                if (WebUiHelpers.GetPostBackControl(this.Page) != null)
                {
                    string controlId = WebUiHelpers.GetPostBackControl(this.Page).ID;

                    switch (controlId)
                    {
                        case "resetTable": // Reset to default values
                            colCount = NumberOfCols;
                            rowCount = NumberOfRows;
                            break;

                        // TODO: Set maximum number of rows that can be created
                        case "addRow": // Add new row
                            rowCount = rowCount + 1;
                            break;

                        // TODO: Set maximum number of columns that can be created
                        case "addColumn": // Add new column
                            colCount = colCount + 1;
                            break;

                        case "removeRow": // Remove 1 row
                            // Has to have at least 1 row
                            if (rowCount > 1)
                            {
                                rowCount = rowCount - 1;
                            }
                            break;

                        case "removeColumn": // Remove 1 column
                            // Has to have at least 2 columns
                            if (colCount > 2)
                            {
                                colCount = colCount - 1;
                            }
                            break;

                        default:
                            break;
                    }
                }
                // Add grid based on new rowCount/colCount values
                tablePanel.Controls.Add(XmlHelpers.DataSetToTable(XmlHelpers.DefaultDataSet(rowCount, colCount)));//, UniqueID));

            }
        }
    }
}
