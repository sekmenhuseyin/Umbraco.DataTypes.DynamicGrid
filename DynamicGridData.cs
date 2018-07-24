using System.Xml;

namespace Umbraco.DataTypes.DynamicGrid
{
    public class DynamicGridData : umbraco.cms.businesslogic.datatype.DefaultData
    {
        public DynamicGridData(umbraco.cms.businesslogic.datatype.BaseDataType DataType) : base(DataType)
        {
        }

        /// =================================================================================
        /// <summary>
        /// to be able to save XML data in the database as actual, nested xml, and not as string-encoded xml (ie not as ![CDATA[)>
        /// </summary>
        /// =================================================================================
        public override XmlNode ToXMl(XmlDocument data)
        {
            if (this.Value == null || string.IsNullOrEmpty(this.Value.ToString())) return base.ToXMl(data);
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(this.Value.ToString());
            return data.ImportNode(xd.DocumentElement, true);
        }
    }
}