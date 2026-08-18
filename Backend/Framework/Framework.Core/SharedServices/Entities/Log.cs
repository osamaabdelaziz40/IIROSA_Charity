// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Log.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Framework.Core.SharedServices.Entities
{
    public class Log
    {
        public Guid Id { get; set; }
        public string Host { get; set; }
        public string MachineName { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string Thread { get; set; }
        public string LogLevel { get; set; }
        public string Logger { get; set; }
        public string UserAgent { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }

        //public string HttpMethod { get; set; }
        public string CallSite { get; set; }
    }
}