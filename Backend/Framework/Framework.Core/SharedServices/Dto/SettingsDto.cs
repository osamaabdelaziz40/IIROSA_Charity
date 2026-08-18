using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Framework.Core.SharedServices.Dto
{
    [Serializable]
    public class SettingsDto
    {
        [HiddenInput]
        public int Id { get; set; }

        public string Name { get; set; }

        public string ValueType { get; set; }

        [Required]
        public string Value { get; set; }

        public string GroupName { get; set; }
    }
}