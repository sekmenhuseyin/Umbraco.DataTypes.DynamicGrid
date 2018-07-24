using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

namespace Umbraco.DataTypes.DynamicGrid.Helpers
{
    public static class XmlHelpers
    {
        /// =================================================================================
        /// <summary>
        /// Reads an ASP table with textboxes and stores the values in a DataSet.
        /// Please note the cells from the first row are stored as DataSet Columns (Must be Unique).
        /// </summary>
        /// <param name="table">An asp:Table to read values from.</param>
        /// <returns></returns>
        /// =================================================================================
        public static DataSet TableToDataSet(Table table)
        {
            DataSet ds = new DataSet("Data");

            DataTable dt = new DataTable("Row");
            ds.Tables.Add(dt);

            //Add headers
            for (int i = 0; i < table.Rows[0].Cells.Count; i++)
            {
                DataColumn col = new DataColumn();
                TextBox headerTxtBox = (TextBox)table.Rows[0].Cells[i].Controls[0];
                TextBox colIdTxtBox = (TextBox)table.Rows[0].Cells[i].Controls[0];
                TextBox colCaptionTxtBox = (TextBox)table.Rows[1].Cells[i].Controls[0];

                col.ColumnName = colIdTxtBox.Text;
                col.Caption = colCaptionTxtBox.Text;
                dt.Columns.Add(col);
            }

            //Add values; skip over row[0] (id) and row[1] (caption)
            for (int i = 2; i < table.Rows.Count; i++)
            {
                DataRow valueRow = dt.NewRow();
                for (int x = 0; x < table.Rows[i].Cells.Count; x++)
                {
                    TextBox valueTextBox = (TextBox)table.Rows[i].Cells[x].Controls[0];
                    valueRow[x] = valueTextBox.Text;
                }
                dt.Rows.Add(valueRow);
            }

            return ds;
        }

        /// =================================================================================
        /// <summary>
        /// Reads values from a DataSet and displays it in an asp:Table
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        /// =================================================================================
        public static Table DataSetToTable(DataSet ds)//, string UniqueID)
        {
            //UniqueID = Guid.NewGuid().ToString();// string.Empty;

            DataTable dt = ds.Tables["Row"];
            Table newTable = new Table { ID = "DynamicGridTable" };// +UniqueID;

            /////////////////// Add IDs row
            TableRow headerRow = new TableRow();
            //auto generated haeder id row
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                TableCell headerCell = new TableCell();
                TextBox headerTxtBox = new TextBox
                {
                    ID = "HeadersTxtBox" + i.ToString(),// +UniqueID;
                    Text = $"C{i}",// dt.Columns[i].ColumnName;
                    Enabled = false//its header: you cannot edit it.
                };
                headerTxtBox.Style.Add("visibility", "hidden");
                headerCell.Controls.Add(headerTxtBox);
                headerRow.Cells.Add(headerCell);
            }

            newTable.Rows.Add(headerRow);

            /////////////////// Add Caption row

            TableRow captionRow = new TableRow();
            //auto generated caption row
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                TableCell captionCell = new TableCell();
                TextBox captionTxtBox = new TextBox
                {
                    ID = "CaptionsTxtBox" + i.ToString(),// +UniqueID;
                    Text = dt.Columns[i].Caption
                };

                captionTxtBox.Font.Bold = true;
                captionTxtBox.Font.Size = new FontUnit(1.1, UnitType.Em);

                captionCell.Controls.Add(captionTxtBox);
                captionRow.Cells.Add(captionCell);
            }

            newTable.Rows.Add(captionRow);

            /////////////////// Add value rows

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                TableRow valueRow = new TableRow();

                //Add cells & textbox to row
                for (int x = 0; x < dt.Columns.Count; x++)
                {
                    TableCell valueCell = new TableCell();
                    TextBox valueTxtBox = new TextBox
                    {
                        ID = "ValueTxtBox" + i.ToString() + i + x + x.ToString(),// +UniqueID;
                        Text = dt.Rows[i][x].ToString()
                    };

                    //Left column bold (as headers).
                    if (x == 0)
                        valueTxtBox.Font.Bold = true;

                    valueCell.Controls.Add(valueTxtBox);
                    valueRow.Cells.Add(valueCell);
                }
                newTable.Rows.Add(valueRow);
            }

            return newTable;
        }

        /// =================================================================================
        /// <summary>
        /// Converts an XML string to a DataSet
        /// </summary>
        /// <param name="xml">The XML string to convert.</param>
        /// <returns>A DataSet built from an XML string.</returns>
        /// =================================================================================
        public static DataSet XMLStringToDataSet(string xml)
        {
            //strip out attributes and use them to populate a dictionary. We'll set DataColumn captions with them later

            Dictionary<string, string> columnCaptions = new Dictionary<string, string>();
            XDocument xDoc = XDocument.Parse(xml);

            IEnumerable<XElement> els =
                (from el in xDoc.Root.Elements().Elements()
                 select el);

            foreach (var x in els)
            {
                if (!columnCaptions.ContainsKey(x.Name.LocalName))
                {
                    var caps = x.Attribute("caption");
                    columnCaptions.Add(x.Name.LocalName, caps != null ? x.Attribute("caption").Value : "CAPTION");
                }
                x.RemoveAttributes();
            }

            StringReader sr = new StringReader(xDoc.ToString());
            DataSet ds = new DataSet("Data");
            ds.ReadXml(sr);

            foreach (DataColumn col in ds.Tables["Row"].Columns)
            {
                if (columnCaptions.ContainsKey(col.ColumnName))
                    col.Caption = columnCaptions[col.ColumnName];
            }

            return ds;
        }

        /// =================================================================================
        /// <summary>
        /// Converts a DataSet into an XML string that can be stored in a database field.
        /// </summary>
        /// <param name="ds">The DataSet to convert.</param>
        /// <returns></returns>
        /// =================================================================================
        public static string DataSetToXMLString(DataSet ds)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(ds.GetXml());

            //StringWriter sw = new StringWriter();
            //XmlTextWriter xw = new XmlTextWriter(sw);

            //XmlDocument xml = _XMLDoc;
            //xml.WriteTo(xw);

            XDocument xd = XDocument.Load(new XmlNodeReader(xmlDoc));
            foreach (var xel in xd.Descendants())
            {
                if (xel.Name.LocalName.StartsWith("C"))
                    xel.Add(new XAttribute("caption", ds.Tables[0].Columns[xel.Name.LocalName].Caption));
            }
            //TODO
            return xd.ToString();

            //return sw.ToString();
        }

        /// =================================================================================
        /// <summary>
        /// Creates the default dataset & sets the default names for the first few columns.
        /// </summary>
        /// <param name="rows">the number of rows to create (header not counted)</param>
        /// <param name="cols">the number of columns to create</param>
        /// <returns>A DataSet with default values.</returns>
        /// =================================================================================
        public static DataSet DefaultDataSet(int rows, int cols)
        {
            DataSet ds = new DataSet("Data");
            DataTable dt = new DataTable("Row");
            ds.Tables.Add(dt);

            // Creates at least 2 columns
            DataColumn col0 = new DataColumn
            {
                Caption = "Label",
                ColumnName = "C0",
                DataType = Type.GetType("System.String")
            };
            dt.Columns.Add(col0);

            DataColumn col1 = new DataColumn
            {
                Caption = "Actual",
                ColumnName = "C1",
                DataType = Type.GetType("System.String")
            };
            dt.Columns.Add(col1);

            // Adds default value for 3rd column
            if (cols > 2)
            {
                DataColumn heightCol = new DataColumn
                {
                    Caption = "Target",
                    ColumnName = "C2",
                    DataType = Type.GetType("System.String")
                };
                dt.Columns.Add(heightCol);
            }
            // Adds default value for 4th column
            if (cols > 3)
            {
                DataColumn depthCol = new DataColumn
                {
                    Caption = "Status Report",
                    ColumnName = "C3",
                    DataType = Type.GetType("System.String")
                };
                dt.Columns.Add(depthCol);
            }
            // If more than 4 columns - create it with name "New" and append incremented number
            if (cols > 4)
            {
                //int newColCount = cols - 4;
                for (int i = 4; i < cols; i++)
                {
                    DataColumn newCol = new DataColumn
                    {
                        Caption = "New Column " + i.ToString(),
                        ColumnName = "C" + i.ToString(),
                        DataType = Type.GetType("System.String")
                    };
                    dt.Columns.Add(newCol);
                }
            }

            // Add rows from specified "rows" parameter
            for (int i = 0; i < rows; i++)
            {
                DataRow newRow = dt.NewRow();
                newRow["C0"] = "Label" + i.ToString();
                newRow["C1"] = "Actual " + i.ToString();
                if (cols > 2)
                {
                    newRow["C2"] = "Target " + i.ToString();
                }
                if (cols > 3)
                {
                    newRow["C3"] = "Status Report " + i.ToString();
                }
                dt.Rows.Add(newRow);
            }
            return ds;
        }

        /// =================================================================================
        /// <summary>
        /// Make sure the content is valid XML.
        /// Do some light-weight string checking before more expensive XElement.Parse
        /// </summary>
        /// =================================================================================
        public static bool IsValidXml(string data)
        {
            // Has to have length to be XML
            if (string.IsNullOrEmpty(data)) return false;
            // If it starts with a < then it probably is XML
            // But also cover the case where there is indeterminate whitespace before the <
            if (data[0] != '<' && data.TrimStart()[0] != '<') return false;
            try
            {
                string isValid = XElement.Parse(data).Value;
                return true;
            }
            catch (System.Xml.XmlException)
            {
                return false;
            }
        }

        public static XmlNode GetXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }
    }
}