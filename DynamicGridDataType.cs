﻿using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace Umbraco.DataTypes.DynamicGrid
{
    public class DynamicGridDataType : umbraco.cms.businesslogic.datatype.AbstractDataEditor, IDataType
    {
        /// =================================================================================
        #region Control Prevalues
        /// =================================================================================

        [DataEditorSetting("Number Of Columns", description = "Default Number of Columns", defaultValue = "4")]
        public string NumberOfCols { get; set; }

        [DataEditorSetting("Number Of Rows", description = "Default Number of Rows", defaultValue = "2")]
        public string NumberOfRows { get; set; }

        /// =================================================================================
        #endregion

        private DynamicGridControl _control = new DynamicGridControl();

        public override Guid Id { get { return new Guid("50665d19-d6bd-4901-a4a7-7e0cc1011504"); } }
        public override string DataTypeName { get { return "Dynamic Grid"; } }

        private umbraco.interfaces.IData _data { get; set; }
        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_data == null)
                    _data = new DynamicGridData(this);

                return _data;
            }
        }

        /// =================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// =================================================================================
        public DynamicGridDataType()
        {
            base.RenderControl = _control;
            _control.Init += new EventHandler(control_Init);
            base.DataEditorControl.OnSave += new umbraco.cms.businesslogic.datatype.AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
        }

        /// =================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// =================================================================================
        void control_Init(object sender, EventArgs e)
        {
            int outInt;

            _control.NumberOfCols = int.TryParse(NumberOfCols, out outInt) ? outInt : 4;
            _control.NumberOfRows = int.TryParse(NumberOfRows, out outInt) ? outInt : 2;
            _control.XmlValue = Data.Value != null ? Data.Value.ToString() : "";
        }

        /// =================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// =================================================================================
        void DataEditorControl_OnSave(EventArgs e)
        {
            Data.Value = _control.XmlValue;
        }

    }
}
