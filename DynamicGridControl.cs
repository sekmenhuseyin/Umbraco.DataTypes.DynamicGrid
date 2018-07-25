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
        /// number of cols in the table
        /// </summary>
        public int NumberOfCols { get; set; }
        /// <summary>
        /// number of rows in the table
        /// </summary>
        public int NumberOfRows { get; set; }
        /// <summary>
        /// the xml that out table ,s de,gned from
        /// </summary>
        public string XmlValue
        {
            get
            {
                string possibleXml = XmlHelpers.DataSetToXMLString(XmlHelpers.TableToDataSet((Table)_tablePanel.FindControl("DynamicGridTable")));
                return XmlHelpers.IsValidXml(possibleXml) ? possibleXml : string.Empty;
            }
            set
            {
                if (XmlHelpers.IsValidXml(value))
                    _xmlValue = value;
            }
        }
        private string _xmlValue { get; set; }
        /// <summary>
        /// A counter that stores the number of columns to be created.
        /// </summary>
        private int ColCount
        {
            get => (int)ViewState["colCount"];
            set => ViewState["colCount"] = value;
        }
        /// <summary>
        /// A counter that stores the number of rows to be created.
        /// </summary>
        private int RowCount
        {
            get => (int)ViewState["rowCount"];
            set => ViewState["rowCount"] = value;
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
                ID = "addColumn",
                Text = "Sütun Ekle"
            };
            _removeColumn = new LinkButton
            {
                ID = "removeColumn",
                Text = "Sütun Kaldır"
            };
            _addRow = new LinkButton
            {
                ID = "addRow",
                Text = "Satır Ekle"
            };
            _removeRow = new LinkButton
            {
                ID = "removeRow",
                CssClass = "DynamicGridControlDeleteSelected",
                Text = "Seçili Satırları Sil",
                OnClientClick = "DeleteCampaignsFromTable();"
            };
            _resetTable = new LinkButton
            {
                ID = "uploadExcel",
                Text = "Excel'den Yükle"
            };
            _tablePanel = new Panel { ID = "PanelPlaceholder", CssClass = "PanelPlaceholder" };

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
                //emtpy data: default rows and cols
                if (string.IsNullOrEmpty(_xmlValue))
                {
                    RowCount = NumberOfRows;
                    ColCount = NumberOfCols;

                    _tablePanel.Controls.Add(
                        XmlHelpers.DataSetToTable(XmlHelpers.DefaultDataSet(RowCount, ColCount)));
                }
                // get from db
                else
                {
                    //Create dataset/table from XML string in database
                    DataSet ds = XmlHelpers.XMLStringToDataSet(_xmlValue);
                    Table dimensionsTable = XmlHelpers.DataSetToTable(ds);
                    _tablePanel.Controls.Add(dimensionsTable);

                    DataTable dt = ds.Tables["Row"];
                    RowCount = dt.Rows.Count;
                    ColCount = dt.Columns.Count;
                }
            }
            // postback: occurs when a link is pressed
            else
            {
                // We're using GetPostBackControl instead of the buttons' click events due to Page Cycle issues
                // See http://stackoverflow.com/questions/2800496/c-counter-requires-2-button-clicks-to-update
                if (WebUiHelpers.GetPostBackControl(this.Page) != null)
                {
                    string controlId = WebUiHelpers.GetPostBackControl(this.Page).ID;

                    switch (controlId)
                    {
                        case "addColumn": // Add new column
                            ColCount = ColCount + 1;
                            break;

                        case "removeColumn": // Remove 1 column
                            // Has to have at least 2 columns
                            if (ColCount > 2)
                            {
                                ColCount = ColCount - 1;
                            }
                            break;

                        case "addRow": // Add new row
                            RowCount = RowCount + 1;
                            break;

                        case "uploadExcel": // upload excel file

                            break;

                        default:
                            break;
                    }
                }
                // Add grid based on new rowCount/colCount values
                _tablePanel.Controls.Add(XmlHelpers.DataSetToTable(XmlHelpers.DefaultDataSet(RowCount, ColCount)));
            }
        }
    }
}