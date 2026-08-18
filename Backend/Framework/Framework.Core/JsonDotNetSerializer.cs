// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonDotNetSerializer.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core
{
    #region usings

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    #endregion usings

    /// <summary>
    ///     The json dot net serializer.
    /// </summary>
    public static class JsonDotNetSerializer
    {
        /// <summary>
        ///     The get json serializer settings.
        /// </summary>
        /// <returns>
        ///     The <see cref="JsonSerializerSettings" />.
        /// </returns>
        public static JsonSerializerSettings SerializerSettings =>
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,

                // for everything else
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "dd/MM/yyyy",

                // MaxDepth = 3,
                Formatting = Formatting.None
            };
    }
}