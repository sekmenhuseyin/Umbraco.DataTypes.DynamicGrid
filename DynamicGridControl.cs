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
        /// <summary>
        /// this sapces has to here otherwise only one spacer is added to page
        /// </summary>
        private HtmlGenericControl LinksSpacer => new HtmlGenericControl("span") { InnerHtml = "&nbsp;|&nbsp;" };

        /// <summary>
        /// Panel to hold the controls
        /// </summary>
        private Panel _tablePanel;

        /// <summary>
        /// number of cols in the table
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int NumberOfCols { get; set; }

        /// <summary>
        /// number of rows in the table
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
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

        // ReSharper disable once InconsistentNaming
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
            UpdateMode = UpdatePanelUpdateMode.Conditional;
            // Initialize buttons & Panel
            var addColumn = new LinkButton
            {
                ID = "addColumn",
                Text = "Sütun Ekle"
            };
            var removeColumn = new LinkButton
            {
                ID = "removeColumn",
                Text = "Sütun Kaldır"
            };
            var addRow = new LinkButton
            {
                ID = "addRow",
                Text = "Satır Ekle"
            };
            var removeRow = new LinkButton
            {
                ID = "removeRow",
                CssClass = "DynamicGridControlDeleteSelected",
                Text = "Seçili Satırları Sil",
                OnClientClick = "DeleteCampaignsFromTable();"
            };
            var uploadExcel = new FileUpload
            {
                ID = "uploadExcel",
                CssClass = "DynamicGridControlUploadFileSelect"
            };
            var uploadExcelButton = new LinkButton
            {
                ID = "uploadExcelButton",
                Text = "Excel'den Yükle",
                CssClass = "DynamicGridControlUploadExcelButton"
            };
            _tablePanel = new Panel
            {
                ID = "PanelPlaceholder",
                CssClass = "PanelPlaceholder"
            };
            // Add to Update Panel
            _tablePanel.Controls.Add(addColumn);
            _tablePanel.Controls.Add(LinksSpacer);
            _tablePanel.Controls.Add(removeColumn);
            _tablePanel.Controls.Add(LinksSpacer);
            _tablePanel.Controls.Add(addRow);
            _tablePanel.Controls.Add(LinksSpacer);
            _tablePanel.Controls.Add(removeRow);
            _tablePanel.Controls.Add(LinksSpacer);
            _tablePanel.Controls.Add(uploadExcel);
            _tablePanel.Controls.Add(uploadExcelButton);
            //add panel to page
            ContentTemplateContainer.Controls.Add(_tablePanel);
            // if no postback
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
            // if postback: occurs when a link is pressed
            else
            {
                // We're using GetPostBackControl instead of the buttons' click events due to Page Cycle issues
                // See http://stackoverflow.com/questions/2800496/c-counter-requires-2-button-clicks-to-update
                if (WebUiHelpers.GetPostBackControl(Page) != null)
                {
                    string controlId = WebUiHelpers.GetPostBackControl(Page).ID;

                    switch (controlId)
                    {
                        case "addColumn": // Add new column
                            ColCount = ColCount + 1;
                            ScriptManager.RegisterStartupScript(this, typeof(Page), "UpdateMsg", "DynamicGridControlSpeechBubble('Başarılı', 'Sütun eklendi', 'save');", true);
                            break;

                        case "removeColumn": // Remove 1 column
                            // Has to have at least 2 columns
                            if (ColCount > 2)
                            {
                                ColCount = ColCount - 1;
                                ScriptManager.RegisterStartupScript(this, typeof(Page), "UpdateMsg", "DynamicGridControlSpeechBubble('Başarılı', 'Sütun silindi', 'save');", true);
                            }
                            else
                                ScriptManager.RegisterStartupScript(this, typeof(Page), "UpdateMsg", "DynamicGridControlSpeechBubble('Hata', 'Sütun silinmedi', 'error');", true);
                            break;

                        case "addRow": // Add new row
                            RowCount = RowCount + 1;
                            ScriptManager.RegisterStartupScript(this, typeof(Page), "UpdateMsg", "DynamicGridControlSpeechBubble('Başarılı', 'Satır eklendi', 'save');", true);
                            break;
                    }
                }
                // Add grid based on new rowCount/colCount values
                _tablePanel.Controls.Add(XmlHelpers.DataSetToTable(XmlHelpers.DefaultDataSet(RowCount, ColCount)));
            }
        }
    }
}