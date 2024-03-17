using SnitzCore.Data.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SnitzCore.Data.Extensions
{
    public static class PropertyExtensions
    {

        public static bool IsNumeric(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            return value.All(char.IsNumber);
        }
        public static T? GetAttribute<T>(this MemberInfo member, bool isRequired)
            where T : Attribute
        {
            var attribute = member.GetCustomAttributes(typeof(T), false).SingleOrDefault();

            if (attribute == null && isRequired)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The {0} attribute must be defined on member {1}",
                        typeof(T).Name,
                        member.Name));
            }

            return (T?)attribute;
        }

        public static string GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    nameof(propertyExpression));
            }

            var attr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        public static string GetPropertyDisplayName<T>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    nameof(memberInfo));
            }

            var attr = memberInfo.GetAttribute<ProfileDisplayAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        public static string? GetPropertyDisplayCheck<T>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    nameof(memberInfo));
            }

            var attr = memberInfo.GetAttribute<ProfileDisplayAttribute>(false);
            return attr?.DisplayCheck;
        }
        public static bool PropertyIsSocialMedia(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;

            return orderAttr.SocialLink;
        }
        public static MemberLayout PropertyLayoutSection(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;

            return orderAttr.LayoutSection;
        }
        public static bool PropertyIsPersonal(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;

            return orderAttr.SocialLink;
        }
        public static string? GetPropertyRequiredCheck<T>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    nameof(memberInfo));
            }

            var attr = memberInfo.GetAttribute<ProfileDisplayAttribute>(false);
            if (attr == null)
            {
                return null;
            }

            return attr.RequiredCheck;
        }

        public static int PropertyOrder(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;

            return orderAttr.Order;
        }

        public static bool PropertyReadOnly(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;
            return orderAttr.ReadOnly;
        }
        public static bool PropertyIsPrivate(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;
            return orderAttr.Private;
        }
        public static bool SystemProperty(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;
            return orderAttr.SystemField;
        }

        public static string PropertyFieldType(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;
            return orderAttr.FieldType;
        }
        public static string? GetSelectEnum(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault()!;
            return orderAttr.SelectEnum;
        }
        public static MemberInfo? GetPropertyInformation(Expression propertyExpression)
        {
            Debug.Assert(propertyExpression != null, "propertyExpression != null");
            MemberExpression? memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null)
            {
                if (propertyExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpr)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr is { Member.MemberType: MemberTypes.Property })
            {
                return memberExpr.Member;
            }

            return null;
        }


    }
}
