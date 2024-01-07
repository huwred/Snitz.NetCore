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
        public static T GetAttribute<T>(this MemberInfo member, bool isRequired)
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

            return (T)attribute;
        }

        public static string GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
            }

            var attr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        public static string? GetPropertyDisplayName<T>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
            }

            var attr = memberInfo.GetAttribute<ProfileDisplayAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName ?? memberInfo.Name;
        }

        public static string? GetPropertyDisplayCheck<T>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
            }

            var attr = memberInfo.GetAttribute<ProfileDisplayAttribute>(false);
            return attr.DisplayCheck;
        }

        public static string GetPropertyRequiredCheck<T>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
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
                .SingleOrDefault();
            int output = orderAttr != null ? orderAttr.Order : Int32.MaxValue;
            return output;
        }

        public static bool PropertyReadOnly(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault();
            bool output = orderAttr != null ? orderAttr.ReadOnly : false;
            return output;
        }
        public static bool PropertyIsPrivate(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault();
            bool output = orderAttr != null ? orderAttr.Private : false;
            return output;
        }
        public static bool SystemProperty(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault();
            bool output = orderAttr != null ? orderAttr.SystemField : false;
            return output;
        }

        public static string? PropertyFieldType(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault();
            return orderAttr?.FieldType;
        }
        public static string? GetSelectEnum(this PropertyInfo propInfo)
        {
            var orderAttr = (ProfileDisplayAttribute)propInfo.GetCustomAttributes(typeof(ProfileDisplayAttribute), true)
                .SingleOrDefault();
            return orderAttr?.SelectEnum;
        }
        public static MemberInfo? GetPropertyInformation(Expression propertyExpression)
        {
            Debug.Assert(propertyExpression != null, "propertyExpression != null");
            MemberExpression memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = propertyExpression as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
            {
                return memberExpr.Member;
            }

            return null;
        }


    }
}
