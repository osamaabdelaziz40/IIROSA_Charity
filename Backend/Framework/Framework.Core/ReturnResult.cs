// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReturnResult.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core
{
    #region usings

    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.Collections.Generic;
    using System.Linq;

    #endregion usings

    /// <summary>
    /// The return result.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class ReturnResult<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReturnResult{T}" /> class.
        /// </summary>
        public ReturnResult()
        {
            this.Value = default(T);
            this.Errors = new List<Item>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnResult{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        public ReturnResult(T defaultValue)
        {
            this.Value = defaultValue;
            this.Errors = new List<Item>();
        }

        /// <summary>
        ///     Gets or sets the model state.
        /// </summary>
        public List<Item> Errors { get; }

        /// <summary>
        ///     The is valid.
        /// </summary>
        public bool IsValid => !this.Errors.Any();

        /// <summary>
        ///     Gets or sets the return value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// The add error item.
        /// </summary>
        /// <param name="key">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void AddErrorItem(string key, string value)
        {
            this.Errors.Add(new Item(key, value));
        }
    }

    /// <summary>
    ///     The return result.
    /// </summary>
    public class ReturnResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReturnResult" /> class.
        ///     Initializes a new instance of the <see cref="ReturnResult{T}" /> class.
        /// </summary>
        public ReturnResult()
        {
            this.Errors = new List<Item>();
        }

        /// <summary>
        ///     Gets or sets the errors.
        /// </summary>
        public List<Item> Errors { get; set; }

        /// <summary>
        ///     The is valid.
        /// </summary>
        public bool IsValid => !this.Errors.Any();

        /// <summary>
        /// The add error item.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void AddErrorItem(string key, string value)
        {
            this.Errors.Add(new Item(key, value));
        }
    }

    /// <summary>
    ///     The model state dictionary extentsions.
    /// </summary>
    public static class ModelStateDictionaryExtentsions
    {
        /// <summary>
        /// The get errors.
        /// </summary>
        /// <param name="modelState">
        /// The model state.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Item> GetErrors(this ModelStateDictionary modelState)
        {
            var errors = new List<Item>();

            foreach (var state in modelState)
            {
                errors.AddRange(state.Value.Errors.Select(error => new Item(state.Key, error.ErrorMessage)));
            }

            return errors;
        }

        /// <summary>
        /// The get errors.
        /// </summary>
        /// <param name="modelState">
        /// The model state.
        /// </param>
        /// <param name="separator">
        /// The separator.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetErrors(this ModelStateDictionary modelState, string separator)
        {
            return string.Join(separator, modelState.GetErrors().Select(e => e.Value));
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="modelState">
        /// The model state.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        public static void Merge(this ModelStateDictionary modelState, ReturnResult result)
        {
            if (result.IsValid)
            {
                return;
            }

            if (result.Errors != null && result.Errors.Any())
            {
                foreach (var item in result.Errors)
                {
                    modelState.AddModelError(item.Name, item.Value);
                }
            }
        }

        public static void Merge(this ReturnResult result, ICollection<Item> errors)
        {
            if (result.IsValid)
            {
                return;
            }

            if (errors != null && errors.Any())
            {
                result.Errors.AddRange(errors);
            }
        }

        public static void Merge(this ModelStateDictionary modelState, ICollection<Item> errors)
        {
            if (modelState.IsValid)
            {
                return;
            }

            if (errors != null && errors.Any())
            {
                foreach (var item in errors)
                {
                    modelState.AddModelError(item.Name, item.Value);
                }
            }
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="modelState">
        /// The model state.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public static void Merge<T>(this ModelStateDictionary modelState, ReturnResult<T> result)
        {
            if (result.IsValid)
            {
                return;
            }

            if (result.Errors != null && result.Errors.Any())
            {
                foreach (var item in result.Errors)
                {
                    modelState.AddModelError(item.Name, item.Value);
                }
            }
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="result1">
        /// The result 1.
        /// </param>
        /// <param name="result2">
        /// The result 2.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="T2">
        /// </typeparam>
        public static void Merge<T, T2>(this ReturnResult<T> result1, ReturnResult<T2> result2)
        {
            if (result2.IsValid)
            {
                return;
            }

            if (result2.Errors != null && result2.Errors.Any())
            {
                foreach (var item in result2.Errors)
                {
                    result1.AddErrorItem(item.Name, item.Value);
                }
            }
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="result1">
        /// The result 1.
        /// </param>
        /// <param name="result2">
        /// The result 2.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public static void Merge<T>(this ReturnResult<T> result1, ReturnResult result2)
        {
            if (result2.IsValid)
            {
                return;
            }

            if (result2.Errors != null && result2.Errors.Any())
            {
                foreach (var item in result2.Errors)
                {
                    result1.AddErrorItem(item.Name, item.Value);
                }
            }
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="result1">
        /// The result 1.
        /// </param>
        /// <param name="result2">
        /// The result 2.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public static void Merge<T>(this ReturnResult result1, ReturnResult<T> result2)
        {
            if (result2.IsValid)
            {
                return;
            }

            if (result2.Errors != null && result2.Errors.Any())
            {
                foreach (var item in result2.Errors)
                {
                    result1.AddErrorItem(item.Name, item.Value);
                }
            }
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="result1">
        /// The result 1.
        /// </param>
        /// <param name="result2">
        /// The result 2.
        /// </param>
        public static void Merge(this ReturnResult result1, ReturnResult result2)
        {
            if (result2.IsValid)
            {
                return;
            }

            if (result2.Errors != null && result2.Errors.Any())
            {
                foreach (var item in result2.Errors)
                {
                    result1.AddErrorItem(item.Name, item.Value);
                }
            }
        }

        /// <summary>
        /// The to string list.
        /// </summary>
        /// <param name="errors">
        /// The errors.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> ToStringList(this List<Item> errors)
        {
            return errors?.Select(e => e.Value).ToList() ?? new List<string>();
        }
    }
}