using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using SnitzCore.Service;

namespace MVCForum.Extensions
{
    public sealed class DisplayAttributeLocalizationProvider : IDisplayMetadataProvider
    {
        private LanguageService _localizer;

        public DisplayAttributeLocalizationProvider(IHtmlLocalizerFactory localizerFactory)
        {
            _localizer = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            if (propertyName != null && IsTransformRequired(propertyName, modelMetadata, propertyAttributes))
            {
                modelMetadata.DisplayName = () => _localizer[propertyName].Value;
            }

            if (context.DisplayMetadata.EnumGroupedDisplayNamesAndValues != null)
            {
                //var test = context
            }
            context.PropertyAttributes?
                .Where(attribute => attribute is DisplayAttribute)
                .Cast<DisplayAttribute>().ToList().ForEach(display =>
                {
                    display.Name = _localizer[display.Name].Value;
                });
        }
        private static bool IsTransformRequired(string propertyName, DisplayMetadata modelMetadata, IReadOnlyList<object> propertyAttributes)
        {
            if (!string.IsNullOrEmpty(modelMetadata.SimpleDisplayProperty))
                return false;

            if (propertyAttributes.OfType<DisplayNameAttribute>().Any())
                return false;

            if (propertyAttributes.OfType<DisplayAttribute>().Any())
                return false;
            if (propertyAttributes.OfType<EnumDataTypeAttribute>().Any())
                return true;
            if (string.IsNullOrEmpty(propertyName))
                return false;

            return true;
        }
    }
}
