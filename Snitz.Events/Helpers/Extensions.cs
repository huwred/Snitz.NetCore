using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snitz.Events.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Snitz.Events.Helpers
{
    public static class EventApplicationHelpers
    {
        public static HtmlString RadioButtonForSelectList<TModel, TProperty>(
            this IHtmlHelper htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            SelectList listOfValues, Enumerators.Position position = Enumerators.Position.Horizontal)
        {
            //var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            var value = ModelMetadata.FromLambdaExpression(
                            expression, htmlHelper.ViewData
                        ).Model;

            var sb = new StringBuilder();

            if (listOfValues != null)
            {

                // Create a radio button for each item in the list 
                foreach (SelectListItem item in listOfValues)
                {
                    if (item.Value == value.ToString())
                    {
                        item.Selected = true;
                    }
                    // Generate an id to be given to the radio button field 
                    var id = string.Format("rb_{0}_{1}",
                      fullName.Replace("[", "").Replace(
                      "]", "").Replace(".", "_"), item.Value);

                    // Create and populate a radio button using the existing html helpers 
                    var label = htmlHelper.Label(id, item.Text); //HttpUtility.HtmlEncode(item.Text)
                    //var radio = htmlHelper.RadioButtonFor(expression, item.Value, new { id = id }).ToHtmlString();
                    var radio = htmlHelper.RadioButton(fullName, item.Value, item.Selected, new { id }).ToHtmlString();

                    // Create the html string that will be returned to the client 
                    // e.g. <input data-val="true" data-val-required=
                    //   "You must select an option" id="TestRadio_1" 
                    //    name="TestRadio" type="radio"
                    //   value="1" /><label for="TestRadio_1">Line1</label> 
                    sb.AppendFormat("<{2} class=\"RadioButton\" name=\"{3}\">{0} {1}</{2}>",
                       radio, label, (position == Enumerators.Position.Horizontal ? "span" : "div"), BbCodeProcessor.Format(fullName));
                }
            }

            return new HtmlString(sb.ToString());
        }
   
      public static HtmlString CheckBoxList(this IHtmlHelper htmlHelper, string name, Dictionary<int, string> listInfo,List<int> subs, object htmlAttributes = null)
      {
          if (String.IsNullOrEmpty(name))
              throw new ArgumentException("The argument must have a value", "name");
          if (listInfo == null)
              throw new ArgumentNullException(nameof(listInfo));
          if (!listInfo.Any())
              throw new ArgumentException("The list must contain at least one value", nameof(listInfo));
   
          StringBuilder sb = new StringBuilder();
   
          foreach (KeyValuePair<int, string> info in listInfo)
          {
                TagBuilder div = new TagBuilder("div");
              div.AddCssClass("checkbox");

                TagBuilder label = new TagBuilder("label");

                TagBuilder builder = new TagBuilder("input");
              if (subs.Contains(info.Key))
              {
                    builder.MergeAttribute("checked", "checked");
                }

              builder.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
                builder.MergeAttribute("type", "checkbox");
              builder.MergeAttribute("value", info.Key.ToString());
              builder.MergeAttribute("name", name);

              label.InnerHtml = builder.ToString(TagRenderMode.Normal) + info.Value;
                div.InnerHtml = label.ToString(TagRenderMode.Normal);
                sb.Append(div.ToString(TagRenderMode.Normal));
              
          }

            return new HtmlString(sb.ToString());
        }

    }
}
