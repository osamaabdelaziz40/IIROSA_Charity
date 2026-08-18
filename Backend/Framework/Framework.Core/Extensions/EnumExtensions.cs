// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumExtensions.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Extensions
{
    using Framework.Core.Data;
    using Framework.Core.DataAnnotations;
    #region usings

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    #endregion

    /// <summary>
    ///     The enum extensions.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// The has.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool Has<T>(this Enum type, T value)
        {
            try
            {
                return ((int)(object)type & (int)(object)value) == (int)(object)value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The is.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool Is<T>(this Enum type, T value)
        {
            try
            {
                return (int)(object)type == (int)(object)value;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// gets description attribute of the enum value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>decription attribute of the enum value</returns>
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                return !(Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute) ? value.ToString() : attribute.Description;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetDescriptionWithCulture(this Enum value, string culture)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                return !(Attribute.GetCustomAttribute(field, typeof(LocalizedDescriptionAttribute)) is LocalizedDescriptionAttribute attribute) ?
                    value.ToString() : attribute.GetDescriptionByCertainCulture(culture);
            }
            else
            {
                return string.Empty;
            }
        }

        public static List<LookupEntityBase> GetEnumLookups(this Type e)
        {
            Array values = Enum.GetValues(e);
            var list = new List<LookupEntityBase>();
            foreach (int val in values)
            {

                var memInfo = e.GetMember(e.GetEnumName(val));
                var descriptionAttribute = memInfo[0]
                    .GetCustomAttributes(typeof(LookupLocalizationAttribute), false)
                    .FirstOrDefault() as LookupLocalizationAttribute;
                var nameAr = memInfo[0].Name;
                var nameEn = memInfo[0].Name;

                if (descriptionAttribute != null)
                {
                    nameAr = descriptionAttribute.NameAr;
                    nameEn = descriptionAttribute.NameEn;
                }

                list.Add(new LookupEntityBase() { Id = val, NameAr = nameAr, NameEn = nameEn, IsActive = true });

            }
            return list;
        }



        public static string GetDisplayName(this Enum value)
        {

            try
            {
                if (value == null)
                {
                    return string.Empty;
                }
                FieldInfo field = value.GetType().GetField(value.ToString());
                if (field != null)
                {
                    return !(Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute attribute) ? value.ToString() : attribute.GetName();
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LookupLocalizationAttribute : Attribute
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }

        public LookupLocalizationAttribute(string nameAr, string nameEn)
        {
            NameAr = nameAr;
            NameEn = nameEn;
        }
    }
}