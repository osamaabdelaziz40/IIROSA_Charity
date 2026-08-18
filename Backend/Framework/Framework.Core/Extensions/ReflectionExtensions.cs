// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Extensions
{
    #region usings

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion usings

    /// <summary>
    ///     The reflection extensions.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// The get attribute.
        /// </summary>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="isRequired">
        /// The is required.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
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

        /// <summary>
        /// The get property display name.
        /// </summary>
        /// <param name="propertyExpression">
        /// The property expression.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static string GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException("No property reference expression was found.", "propertyExpression");
            }

            var attr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        /// <summary>
        /// The get property information.
        /// </summary>
        /// <param name="propertyExpression">
        /// The property expression.
        /// </param>
        /// <returns>
        /// The <see cref="MemberInfo"/>.
        /// </returns>
        public static MemberInfo GetPropertyInformation(Expression propertyExpression)
        {
            Debug.Assert(propertyExpression != null, "propertyExpression != null");
            var memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null)
            {
                var unaryExpr = propertyExpression as UnaryExpression;
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