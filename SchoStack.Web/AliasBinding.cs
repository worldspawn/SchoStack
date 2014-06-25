﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SchoStack.Web
{
    /// <summary>
    /// Allows you to create aliases that can be used for model properties at
    /// model binding time (i.e. when data comes in from a request).
    /// 
    /// The type needs to be using the DefaultModelBinderEx model binder in 
    /// order for this to work.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class BindAliasAttribute : Attribute
    {
        public BindAliasAttribute(string alias)
        {
            //ommitted: parameter checking
            Alias = alias;
        }
        public string Alias { get; private set; }
    }

    internal sealed class AliasedPropertyDescriptor : PropertyDescriptor
    {
        public PropertyDescriptor Inner { get; private set; }

        public AliasedPropertyDescriptor(string alias, PropertyDescriptor inner)
            : base(alias, null)
        {
            Inner = inner;
        }

        public override bool CanResetValue(object component)
        {
            return Inner.CanResetValue(component);
        }

        public override Type ComponentType
        {
            get { return Inner.ComponentType; }
        }

        public override object GetValue(object component)
        {
            return Inner.GetValue(component);
        }

        public override bool IsReadOnly
        {
            get { return Inner.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return Inner.PropertyType; }
        }

        public override void ResetValue(object component)
        {
            Inner.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            Inner.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return Inner.ShouldSerializeValue(component);
        }
    }

    public class AliasModelBinder : DefaultModelBinder
    {
        protected override PropertyDescriptorCollection GetModelProperties(ControllerContext controllerContext,ModelBindingContext bindingContext)
        {
            var toReturn = base.GetModelProperties(controllerContext, bindingContext);
            var additional = new List<PropertyDescriptor>();

            //now look for any aliasable properties in here
            foreach (var p in this.GetTypeDescriptor(controllerContext, bindingContext).GetProperties().Cast<PropertyDescriptor>())
            {
                foreach (var attr in p.Attributes.OfType<BindAliasAttribute>())
                {
                    additional.Add(new AliasedPropertyDescriptor(attr.Alias, p));

                    if (bindingContext.PropertyMetadata.ContainsKey(p.Name))
                        bindingContext.PropertyMetadata.Add(attr.Alias, bindingContext.PropertyMetadata[p.Name]);
                }
            }

            return new PropertyDescriptorCollection(toReturn.Cast<PropertyDescriptor>().Concat(additional).ToArray());
        }
    }
}
