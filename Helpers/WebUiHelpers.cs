using System.Web.UI;

namespace Umbraco.DataTypes.DynamicGrid.Helpers
{
    public static class WebUiHelpers
    {
        /// =================================================================================
        /// <summary>
        /// Reads which control triggered a postback.
        /// </summary>
        /// <param name="page">The current page, usually this.Page.</param>
        /// <returns>The control which triggered the postback.</returns>
        /// =================================================================================
        public static Control GetPostBackControl(Page page)
        {
            Control control = null;

            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if (ctrlname != null && ctrlname != string.Empty)
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }
    }
}