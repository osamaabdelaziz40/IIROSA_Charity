// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiResponse.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    #region usings

    using System.Collections.Generic;

    #endregion usings

    /// <summary>
    /// The api response.
    /// </summary>
    public class ApiResponse
    {
        // Yousra: this property to indicate that the returned message
        // is waiting for user confirmation or just an alert information
        /// <summary>
        /// Gets or sets a value indicating whether confirm.
        /// </summary>
        public bool Confirm { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the model state errors.
        /// </summary>
        public ICollection<Item> ModelStateErrors { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets a value indicating whether success.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }
    }

    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether confirm.
        /// </summary>
        public bool Confirm { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the model state errors.
        /// </summary>
        public ICollection<Item> ModelStateErrors { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets a value indicating whether success.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public T Value { get; set; }
    }

    public class Item
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public Item(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public Item(string name, string value, string errorCode)
        {
            Name = name;
            Value = value;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
        public string ErrorCode { get; set; }
    }

    public static class ApiResponsExtention
    {
        public static ApiResponse<T> MergeReturnResult<T>(this ApiResponse<T> response, ReturnResult<T> returnResult)
        {
            if (returnResult.IsValid)
            {
                response.Value = returnResult.Value;
            }
            else
            {
                if (returnResult.Errors.Count > 0)
                {
                    if (returnResult.Errors.Count == 1)
                    {
                        response.Message = returnResult.Errors[0].Value;
                    }
                    foreach (var error in returnResult.Errors)
                    {
                        response.ModelStateErrors.Add(new Item(error.Name, error.Value));
                    }
                }
            }
            response.Success = returnResult.IsValid;
            return response;
        }

        public static ApiResponse MergeReturnResult(this ApiResponse response, ReturnResult returnResult)
        {
            if (!returnResult.IsValid && returnResult.Errors.Count > 0)
            {
                if (returnResult.Errors.Count == 1)
                {
                    response.Message = returnResult.Errors[0].Value;
                }
                foreach (var error in returnResult.Errors)
                {
                    response.ModelStateErrors.Add(new Item(error.Name, error.Value));
                }
            }
            response.Success = returnResult.IsValid;
            return response;
        }

        public static ModelStateDictionary MergeToModelState<T>(this ApiResponse<T> response, ModelStateDictionary modelState)
        {
            foreach (var item in response.ModelStateErrors)
            {
                modelState.AddModelError(item.Name, item.Value);
            }
            return modelState;
        }

        public static ModelStateDictionary MergeToModelState(this ApiResponse response, ModelStateDictionary modelState)
        {
            foreach (var item in response.ModelStateErrors)
            {
                modelState.AddModelError(item.Name, item.Value);
            }
            return modelState;
        }
    }
}