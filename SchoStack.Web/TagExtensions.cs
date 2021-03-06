using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using FubuCore.Reflection;
using HtmlTags;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Url;

namespace SchoStack.Web.Html
{
    public static class TagExtensions
    {
        public static IEnumerable<LoopItem<TModel, TData>> Loop<TModel, TData>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, IEnumerable<TData>>> listExpression)
        {
            var enumerable = listExpression.Compile().Invoke(htmlHelper.ViewData.Model);
            var listFunc = LoopItem<TModel, TData>.GetCurrentIndexedExpressionWithIntParam(listExpression).Compile();
            return LoopItem<TModel, TData>.LoopItems(htmlHelper, listExpression, listFunc, enumerable);
        }

        public static HtmlProfileContext Profile(this HtmlHelper helper, IHtmlProfile profile)
        {
            var existingContext = helper.ViewContext.HttpContext.Items[HtmlProfileContext.SchostackWebProfile] as HtmlProfileContext;
            var htmlProfileContext = new HtmlProfileContext(helper, profile, existingContext);
            helper.ViewContext.HttpContext.Items[HtmlProfileContext.SchostackWebProfile] = htmlProfileContext;
            return htmlProfileContext;
        }

        public static HtmlTag Input<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, object>> expression)
        {
            var tag = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            return tag.GenerateInputFor(helper.ViewContext, expression);
        }

        public static HtmlTag Display<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, object>> expression)
        {
            var tag = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            return tag.GenerateDisplayFor(helper.ViewContext, expression);
        }

        public static HtmlTag Label<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, object>> expression)
        {
            var tag = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            return tag.GenerateLabelFor(helper.ViewContext, expression);
        }

        public static LiteralTag ValidationSummary(this HtmlHelper htmlHelper)
        {
            return ValidationSummary(htmlHelper, false);
        }

        public static LiteralTag ValidationSummary(this HtmlHelper htmlHelper, bool excludePropertyErrors)
        {
            var val = ValidationExtensions.ValidationSummary(htmlHelper, excludePropertyErrors);
            if (val != null)
                return new LiteralTag(val.ToHtmlString());

            var valtag = new DivTag().AddClass(HtmlHelper.ValidationSummaryCssClassName);
            valtag.Append(new HtmlTag("ul").Append(new HtmlTag("li").Style("display", "none")));
            if (!excludePropertyErrors)
                valtag.Data("valmsg-summary", "true");

            return new LiteralTag(valtag.ToHtmlString());
        }

        public static LiteralTag ValidationMessage<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return ValidationMessage(htmlHelper, expression, null);
        }

        public static LiteralTag ValidationMessage<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string message)
        {
            var reqName = RequestData.GetName(ReflectionHelper.GetAccessor(expression));
            var val = ValidationExtensions.ValidationMessage(htmlHelper, reqName, message);
            if (val != null)
                return new LiteralTag(val.ToHtmlString());
            return new LiteralTag("");
        }

        public static HtmlTag Submit(this HtmlHelper htmlHelper, string text)
        {
            var tag = TagGen().GenerateTagFor(htmlHelper.ViewContext, () => new HtmlTag("input").Attr("type", "submit").Attr("value", text));
            return tag;
        }

        public static LinkTag Link(this HtmlHelper htmlHelper, string text, string action)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var tag = TagGen().GenerateTagFor(htmlHelper.ViewContext, () => new LinkTag(text, urlHelper.Action(action)));
            return tag;
        }

        public static LinkTag Link(this HtmlHelper htmlHelper, string text, string action, string controller, object routeValues = null)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var tag = TagGen().GenerateTagFor(htmlHelper.ViewContext, () => new LinkTag(text, urlHelper.Action(action, controller, routeValues)));
            return tag;
        }

        public static HtmlTag Tag(this HtmlHelper htmlHelper, string tagName)
        {
            var tag = TagGen().GenerateTagFor(htmlHelper.ViewContext, () => new HtmlTag("tag"));
            return tag;
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string action)
        {
            return ChildActionExtensions.Action(htmlHelper, action);
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string action, string controller)
        {
            return ChildActionExtensions.Action(htmlHelper, action, controller);
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string action, string controller, object routevalues)
        {
            return ChildActionExtensions.Action(htmlHelper, action, controller, routevalues);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partial)
        {
            return PartialExtensions.Partial(htmlHelper, partial);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partial, object model)
        {
            return PartialExtensions.Partial(htmlHelper, partial, model);
        }

        public static string Class(this HtmlHelper htmlHelper, bool condition, string className)
        {
            return condition ? className : null;
        }

        public static TagGenerator TagGen()
        {
            return new TagGenerator(HtmlConventionFactory.HtmlConventions);
        }

    }
   
}

namespace SchoStack.Web.Html.Url
{
    public static class TagExtensions
    {
        public static MvcHtmlString Action<T>(this HtmlHelper htmlHelper, T model)
        {
            var factory = ActionFactory.Actions[typeof(T)];
            var url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            return ChildActionExtensions.Action(htmlHelper, factory.Action, factory.Controller, UrlExtensions.GenerateDict(model, url));
        }

        public static LinkTag Link<T>(this HtmlHelper htmlHelper, T model, string text)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var tag = TagGen().GenerateTagFor(htmlHelper.ViewContext, () => new LinkTag(text, urlHelper.For(model)));
            return tag;
        }

        public static TagGenerator TagGen()
        {
            return new TagGenerator(HtmlConventionFactory.HtmlConventions);
        }
    }
}

namespace SchoStack.Web.Html.Form
{
    public static class TagExtensions
    {
        public static MvcForm Form(this HtmlHelper htmlHelper, string action, string controller, object routesValues = null, Action<FormTag> modifier = null)
        {
            action = action ?? htmlHelper.ViewContext.RequestContext.RouteData.GetRequiredString("action");
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            modifier = modifier ?? (x => { });
            var url = urlHelper.Action(action, controller, routesValues);
            return GenerateForm(null, htmlHelper.ViewContext, modifier, url);
        }

        public static MvcForm Form<TInput>(this HtmlHelper htmlHelper, object routesValues = null, Action<FormTag> modifier = null)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            modifier = modifier ?? (x => { });
            var url = urlHelper.Action(htmlHelper.ViewContext.RequestContext.RouteData.GetRequiredString("action"), routesValues);
            return GenerateForm(typeof(TInput), htmlHelper.ViewContext, modifier, url);
        }

        public static MvcForm Form<TInput>(this HtmlHelper htmlHelper, string action, object routesValues = null, Action<FormTag> modifier = null)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            modifier = modifier ?? (x => { });
            var url = urlHelper.Action(action, routesValues);
            return GenerateForm(typeof(TInput), htmlHelper.ViewContext, modifier, url);
        }

        public static MvcForm Form<TInput>(this HtmlHelper htmlHelper, string action, string controller, object routesValues = null, Action<FormTag> modifier = null)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            modifier = modifier ?? (x => { });
            var url = urlHelper.Action(action, controller, routesValues);
            return GenerateForm(typeof(TInput), htmlHelper.ViewContext, modifier, url);
        }

        public static HtmlTag FormEnd(this HtmlHelper htmlHelper)
        {
            htmlHelper.ViewContext.HttpContext.Items.Remove(TagGenerator.FORMINPUTTYPE);
            return new LiteralTag("</form>");
        }

        public static MvcForm GenerateForm(Type inputType, ViewContext viewContext, Action<FormTag> modifier, string url)
        {
            viewContext.RequestContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = inputType;
            var tagGenerator = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            var tag = tagGenerator.GenerateTagFor(viewContext, () => (FormTag) new FormTag(url).NoClosingTag());
            modifier(tag);
            viewContext.Writer.WriteLine(tag);
            return new InputTypeMvcForm(viewContext);
        }
    }
}

namespace SchoStack.Web.Html.UrlForm
{
    public static class TagExtensions
    {
        public static MvcForm Form<TInput>(this HtmlHelper htmlHelper) where TInput : new()
        {
            return Form(htmlHelper, new TInput());
        }

        public static MvcForm Form<TInput>(this HtmlHelper htmlHelper, Action<FormTag> modifier) where TInput : new()
        {
            return Form(htmlHelper, new TInput(), modifier);
        }

        public static MvcForm Form<TInput>(this HtmlHelper htmlHelper, TInput model)
        {
            return Form(htmlHelper, model, begin => { });
        }

        public static MvcForm Form<TInput>(this HtmlHelper htmlHelper, TInput model, Action<FormTag> modifier)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.For(model);
            return Html.Form.TagExtensions.GenerateForm(model.GetType(), htmlHelper.ViewContext, modifier, url);
        }
        
        public static HtmlTag FormEnd(this HtmlHelper htmlHelper)
        {
            htmlHelper.ViewContext.HttpContext.Items.Remove(TagGenerator.FORMINPUTTYPE);
            return new LiteralTag("</form>");
        }
    }
}