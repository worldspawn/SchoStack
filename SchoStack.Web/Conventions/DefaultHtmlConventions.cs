using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HtmlTags;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Extensions;

namespace SchoStack.Web.Conventions
{
    public class DefaultHtmlConventions : HtmlConvention
    {
        public DefaultHtmlConventions()
        {
            Displays.Always.BuildBy(req => new HtmlTag("span").Text(req.GetValue<string>()));
            Labels.Always.BuildBy(req => new HtmlTag("label").Attr("for", req.Id).Text(req.Accessor.InnerProperty.Name.BreakUpCamelCase()));
            Inputs.Always.BuildBy(req =>
            {
                return new TextboxTag().Attr("value", req.GetAttemptedValue() ?? req.GetValue<string>());
            });

            Inputs.If<bool>().BuildBy(req =>
            {
                var attemptedVal = req.GetAttemptedValue();
                var isChecked = attemptedVal != null ? attemptedVal.Split(',').First() == Boolean.TrueString: req.GetValue<bool>();
                var check = new CheckboxTag(isChecked).Attr("value", true);
                var hidden = new HiddenTag().Attr("name", req.Name).Attr("value", false);
                return check.After(hidden);
            });

            Inputs.If<IEnumerable<SelectListItem>>().BuildBy(BuildSelectList);
            Inputs.If<MultiSelectList>().BuildBy(BuildSelectList);
            
            All.Always.Modify((h, r) =>
            {
                h.Id((string.IsNullOrEmpty(r.Id) ? null : r.Id) ?? (string.IsNullOrEmpty(h.Id()) ? null : h.Id()));
                if (h.IsInputElement())
                {
                    h.Attr("name", r.Name ?? (string.IsNullOrEmpty(h.Attr("name")) ? null : h.Attr("name")));
                }
            });

            Labels.Always.Modify((h, r) => h.Id(r.Id + "_" + "Label"));
            Displays.Always.Modify((h, r) => h.Id(r.Id + "_" + "Display"));

            Inputs.Always.Modify((h, r) =>
            {
                //Validation class
                ModelState modelState;
                if (r.ViewContext.ViewData.ModelState.TryGetValue(r.Name, out modelState) && modelState.Errors.Count > 0)
                {
                    h.AddClass(HtmlHelper.ValidationInputCssClassName);
                }
            });
        }

        private static HtmlTag BuildSelectList(RequestData req)
        {
            return new DefaultSelectListBuilder<SelectListItem>().Build(req);
        }
    }
}