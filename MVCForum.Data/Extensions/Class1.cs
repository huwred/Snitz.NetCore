using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Data.Extensions
{
    /// <summary>
    /// Allows a condtional validation decorator on class properties
    /// so you can decorate as follows
    /// </summary>
    /// <example>
    /// [RequiredIf("TopicId", 0)]
    /// public string Subject { get; set; }
    /// This means when binding an editor to Subject, it is only required if the objects topicid
    /// is non zero, ie a new topic. This allows us to reuse an object for different scenarios like
    /// posting topics and replys using the same form and post object
    /// It also allows us to use the STRREQ data from FORUM_CONFIG_NEW and make properties on the object required.
    /// </example>
    public class RequiredIfAttribute : ValidationAttribute
    {
        // Note: we don't inherit from RequiredAttribute as some elements of the MVC
        // framework specifically look for it and choose not to add a RequiredValidator
        // for non-nullable fields if one is found. This would be invalid if we inherited
        // from it as obviously our RequiredIf only applies if a condition is satisfied.
        // Therefore we're using a private instance of one just so we can reuse the IsValid
        // logic, and don't need to rewrite it.
        private RequiredAttribute innerAttribute = new RequiredAttribute();
        public string DependentProperty { get; set; }
        public object TargetValue { get; set; }
        public string Res { get; set; }
        public string Type { get; set; }

        public RequiredIfAttribute(string dependentProperty, object targetValue, string resource = "", string resSet = "ErrorMessage")
        {
            this.DependentProperty = dependentProperty;
            this.TargetValue = targetValue;
            this.Res = resource;
            this.Type = resSet;

        }

        protected override ValidationResult  IsValid(object value,ValidationContext validationContext)
        {
            if (DependentProperty.StartsWith("STRREQ"))
            {
                var httpContextAccessor = (IHttpContextAccessor)validationContext.GetService(typeof(IHttpContextAccessor));
                var config = (ISnitzConfig)validationContext.GetService(typeof(ISnitzConfig));
                var user = httpContextAccessor.HttpContext.User;
                if (Roles.IsUserInRole(user.Identity.Name, "Administrator"))
                {
                    return true;
                }
                if (config.GetValue(DependentProperty) == ((int)TargetValue).ToString())
                {
                    return innerAttribute.IsValid(value);
                }
                return new ValidationResult();
            }

        }

        public override string FormatErrorMessage(string name)
        {
            if (Res != "")
                ErrorMessage = LangResources.Utility.ResourceManager.GetLocalisedString(Res, Type);
            return base.FormatErrorMessage(name);
        }

    }


}
