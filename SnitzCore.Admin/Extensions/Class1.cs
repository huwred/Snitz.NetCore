using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.BackOffice.Extensions
{
    public static class HtmlExtensions
    {
        public static HtmlString ConfigToggle(this IHtmlHelper helper, string labeltext, string key, string disabled = "", string labelCss = "control-label col-xs-8 col-sm-3", string controlCss = "col-xs-4 col-sm-1", bool hiddeninput = true)
        {
            //string value = SnitzConfig.GetValue(key.ToUpper());

            //TagBuilder controlGroup = new TagBuilder("div");
            ////controlGroup.AddCssClass("form-group");

            //TagBuilder label = new TagBuilder("label");
            //label.AddCssClass("control-label");
            //label.AddCssClass(labelCss);
            //label.InnerHtml = labeltext;

            //TagBuilder controls = new TagBuilder("div");
            //controls.AddCssClass(controlCss);

            //TagBuilder input = new TagBuilder("input");
            //input.AddCssClass("yesno-checkbox " + disabled);
            //if (!String.IsNullOrEmpty(disabled))
            //{
            //    input.Attributes.Add("disabled", "");
            //}
            //input.Attributes["data-size"] = "mini";
            //input.Attributes["type"] = "checkbox";
            //input.Attributes["id"] = key.ToLower();
            //input.Attributes["name"] = key.ToLower();
            //if (value == "1")
            //{
            //    input.Attributes["checked"] = "true";
            //}

            //input.Attributes["value"] = "1";

            //TagBuilder hidden = new TagBuilder("input");
            //hidden.Attributes["type"] = "hidden";
            //hidden.Attributes["id"] = "hdn" + key.ToLower();
            //hidden.Attributes["name"] = key.ToLower();
            //hidden.Attributes["value"] = "0";

            //controls.InnerHtml += input.ToString(TagRenderMode.SelfClosing);
            //if (hiddeninput)
            //    controls.InnerHtml += hidden;
            //controlGroup.InnerHtml += label;
            //controlGroup.InnerHtml += controls;
            //var helpString = ResourceManager.GetKey(key);
            //if (helpString != null)
            //{
            //    TagBuilder col = new TagBuilder("div");
            //    //col.AddCssClass("col-xs-1");
            //    TagBuilder help = new TagBuilder("i");
            //    help.AddCssClass("fa fa-question-circle fa-1_5x pull-left");
            //    help.Attributes["data-toggle"] = "tooltip";
            //    help.Attributes["data-html"] = "true";
            //    help.Attributes["style"] = "margin-top:7px";
            //    help.Attributes["title"] = BbCodeProcessor.Format(helpString,true,true);
            //    col.InnerHtml += help;
            //    controlGroup.InnerHtml += col.InnerHtml;
            //}


            //return HtmlString.Create(controlGroup.InnerHtml);
        }
        /// <summary>
        /// Render a Text input for a CONFIG_VARIABLE
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="labeltext">Text for the associated label</param>
        /// <param name="key">CONFIG_VARIABLE</param>
        /// <param name="placeholder">TExt to display as placeholder</param>
        /// <param name="disabled">set to true to disable the control</param>
        /// <param name="labelCss">non default label css</param>
        /// <param name="controlCss">non default control css</param>
        /// <returns></returns>
        public static HtmlString ConfigString(this HtmlHelper helper, string labeltext, string key, bool disabled = false, string placeholder = "", string labelCss = "control-label col-xs-5 col-sm-3", string controlCss = "col-xs-6")
        {
            TagBuilder controlGroup = new TagBuilder("div");
            controlGroup.AddCssClass("form-group");

            TagBuilder label = new TagBuilder("label");
            label.AddCssClass("control-label");
            label.AddCssClass(labelCss);
            label.InnerHtml.Append(labeltext);

            TagBuilder controls = new TagBuilder("div");
            controls.AddCssClass(controlCss);

            TagBuilder input = new TagBuilder("input");
            
            if (disabled)
            {
                input.Attributes.Add("disabled", "");
                input.AddCssClass("form-control disabled");
            }
            else
            {
                input.AddCssClass("form-control");
            }

            if (!String.IsNullOrEmpty(placeholder))
            {
                input.Attributes["placeholder"] = placeholder;
            }
            input.Attributes["type"] = "text";
            input.Attributes["id"] = key.ToLower();
            input.Attributes["name"] = key.ToLower();
            input.Attributes["value"] = ClassicConfig.GetValue(key.ToUpper());
            controls.InnerHtml.Append(TagRenderMode.SelfClosing);
            controlGroup.InnerHtml += label;
            controlGroup.InnerHtml.Append(controls.);

            return new HtmlString(controlGroup.ToString());
        }

        /// <summary>
        /// Renders a CONFIG VARIABLE (key) as number input
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="labeltext">Text for the associated label</param>
        /// <param name="key">CONFIG_VARIABLE</param>
        /// <param name="min">Minumum no allowed</param>
        /// <param name="max">Maximum no allowed</param>
        /// <param name="step">Increment step for number buttons</param>
        /// <param name="disabled">set to true to disable the control</param>
        /// <param name="labelCss">non default label css</param>
        /// <param name="controlCss">non default control css</param>
        /// <returns></returns>
        public static HtmlString ConfigInt(this HtmlHelper helper, string labeltext, string key,int min, int max, string step, bool disabled = false, string labelCss = "control-label col-xs-5 col-sm-3", string controlCss = "col-xs-2")
        {
            TagBuilder controlGroup = new TagBuilder("div");
            controlGroup.AddCssClass("form-group");

            TagBuilder label = new TagBuilder("label");
            label.AddCssClass("control-label");
            label.AddCssClass(labelCss);
            label.InnerHtml = labeltext;

            TagBuilder controls = new TagBuilder("div");
            controls.AddCssClass(controlCss);

            TagBuilder input = new TagBuilder("input");

            if (disabled)
            {
                input.AddCssClass("form-control disabled");
                input.Attributes.Add("disabled", "");
            }
            else
            {
                input.AddCssClass("form-control");
            }
            input.AddCssClass("inline");
            input.Attributes["type"] = "number";
            input.Attributes["id"] = key.ToLower();
            input.Attributes["name"] = key.ToLower();
            input.Attributes["value"] = ClassicConfig.GetIntValue(key.ToUpper()).ToString();
            
            input.Attributes["min"] = min.ToString();
            input.Attributes["max"] = max.ToString();
            input.Attributes["step"] = step;
            input.Attributes["data-size"] = "sm";

            controls.InnerHtml += input.ToString(TagRenderMode.SelfClosing);
            controlGroup.InnerHtml += label;
            var helpString = BbCodeProcessor.Format(ResourceManager.GetKey(key),true,true);
            if (helpString != null)
            {
                //TagBuilder col = new TagBuilder("div");
                //col.AddCssClass("col-xs-1");
                TagBuilder help = new TagBuilder("i");
                help.AddCssClass("fa fa-question-circle fa-1_5x pull-right flip");
                help.Attributes["data-toggle"] = "tooltip";
                help.Attributes["style"] = "margin-top:7px";
                help.Attributes["title"] = helpString;
                controls.InnerHtml += help;
            }
            controlGroup.InnerHtml += controls;
            return MvcHtmlString.Create(controlGroup.ToString());
        }

    }
}
