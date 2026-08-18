namespace Framework.Core
{
    #region usings

    using Microsoft.Extensions.Logging;

    #endregion usings

    public class ApplicationLogging
    {
        /// <summary>
        /// The logger factory.
        /// </summary>
        private static ILoggerFactory _loggerFactory;

        public static void ConfigureNlogLogger(ILoggerFactory factory)
        {
            _loggerFactory = factory;
        }

        public static ILogger CreateLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }
    }
}