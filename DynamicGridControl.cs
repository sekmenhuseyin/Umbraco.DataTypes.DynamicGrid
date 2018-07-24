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
                string possibleXml = XmlHelpers.DataSetToXmlString(XmlHelpers.TableToDataSet((Table)_tablePanel.FindControl("DynamicGridTable")));
                return XmlHelpers.IsValidXml(possibleXml) ? possibleXml : string.Empty;
            }
            set
            {
                if (XmlHelpers.IsValidXml(value))
                    _xmlValue = value;
            }
        }

        // todo: don't want to use this, but how do i add multiple identical controls to the same page, and still read them with "findcontrol"?
        private string UniqueId => this.UniqueID;

        /// =================================================================================
        /// UI & Counters
        /// =================================================================================

        // Buttons
        private LinkButton _addColumn;
        private LinkButton _removeColumn;
        private LinkButton _addRow;
        private LinkButton _removeRow;
        private LinkButton _resetTable;
        private HtmlGenericControl linksSpacer => new HtmlGenericControl("span") { InnerHtml = "&nbsp;|&nbsp;" };

        //Panel to hold the controls
        private Panel _tablePanel;

        /// <summary>
        /// A counter that stores the number of rows to be created.
        /// </summary>
        private int RowCount
        {
            get => (int)ViewState["rowCount"];
            set => ViewState["rowCount"] = value;
        }

        /// <summary>
        /// A counter that stores the number of columns to be created.
        /// </summary>
        private int colCount
        {
            get => (int)ViewState["colCount"];
            set => ViewState["colCount"] = value;
        }

        /// =================================================================================
        /// when page loads design links and page
        /// =================================================================================
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.UpdateMode = UpdatePanelUpdateMode.Conditional;

            // Initialize buttons & Panel
            _addColumn = new LinkButton
            {
                ID = "addColumn",// + "_" + _uniqueID;// + UniqueID; ;// + _uniqueID;// base.UniqueID;// base.ID;
                Text = "Add Column"
            };
            _removeColumn = new LinkButton
            {
                ID = "removeColumn",// + "_" + _uniqueID;//+ UniqueID; ;// + _uniqueID;//base.UniqueID;//base.ID;
                Text = "Remove Column"
            };
            _addRow = new LinkButton
            {
                ID = "addRow", // + "_" + _uniqueID;//UniqueID; ;//+ _uniqueID;//base.UniqueID;//base.ID;
                Text = "Add Row"
            };
            _removeRow = new LinkButton
            {
                ID = "removeRow", //+ "_" + _uniqueID;//UniqueID; ;// + _uniqueID;// base.UniqueID;// base.ID;
                Text = "Remove Row"
            };
            _resetTable = new LinkButton
            {
                ID = "resetTable",// + "_" + _uniqueID;// + UniqueID; ;//+ _uniqueID;// base.UniqueID;// base.ID;
                Text = "Reset Table"
            };
            _tablePanel = new Panel { ID = "PanelPlaceholder" }; // +_uniqueID;// +UniqueID; // +_uniqueID;// "PanelPlaceHolder_DynamicGridControl";

            // Add to Update Panel
            _tablePanel.Controls.Add(_addColumn);
            _tablePanel.Controls.Add(linksSpacer);
            _tablePanel.Controls.Add(_removeColumn);
            _tablePanel.Controls.Add(linksSpacer);
            _tablePanel.Controls.Add(_addRow);
            _tablePanel.Controls.Add(linksSpacer);
            _tablePanel.Controls.Add(_removeRow);
            _tablePanel.Controls.Add(linksSpacer);
            _tablePanel.Controls.Add(_resetTable);

            base.ContentTemplateContainer.Controls.Add(_tablePanel);

            // no postback
            // could have values in db, or could prepopulate with defaults
            if (!Page.IsPostBack)
            {
                // get from db
                if (!string.IsNullOrEmpty(_xmlValue))
                {
                    //Create dataset/table from XML string in database
                    DataSet ds = XmlHelpers.XmlStringToDataSet(_xmlValue);
                    //DataSet ds = XmlHelpers.XMLStringToDataSet(HttpContext.Current.Server.HtmlDecode(_xmlValue));
                    Table dimensionsTable = XmlHelpers.DataSetToTable(ds);//, UniqueID);
                    _tablePanel.Controls.Add(dimensionsTable);

                    DataTable dt = ds.Tables["Row"];
                    RowCount = dt.Rows.Count;
                    colCount = dt.Columns.Count;
                }
                // prepopulate with defaults
                else
                {
                    RowCount = NumberOfRows;
                    colCount = NumberOfCols;

                    _tablePanel.Controls.Add(XmlHelpers.DataSetToTable(XmlHelpers.DefaultDataSet(RowCount, colCount)));//, UniqueID));
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
                            RowCount = NumberOfRows;
                            break;

                        // TODO: Set maximum number of rows that can be created
                        case "addRow": // Add new row
                            RowCount = RowCount + 1;
                            break;

                        // TODO: Set maximum number of columns that can be created
                        case "addColumn": // Add new column
                            colCount = colCount + 1;
                            break;

                        case "removeRow": // Remove 1 row
                            // Has to have at least 1 row
                            if (RowCount > 1)
                            {
                                RowCount = RowCount - 1;
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
                _tablePanel.Controls.Add(XmlHelpers.DataSetToTable(XmlHelpers.DefaultDataSet(RowCount, colCount)));//, UniqueID));

            }
        }
    }
}
