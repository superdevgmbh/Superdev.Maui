﻿using System.Diagnostics;

namespace Superdev.Maui.SampleApp.Services.Validation
{
    [DebuggerDisplay("ValidationResultDto: Errors={this.Errors.Count}")]
    public class ValidationResultDto
    {
        public ValidationResultDto()
        {
            this.Errors = new Dictionary<string, List<string>>();
        }

        public Dictionary<string, List<string>> Errors { get; set; }

        public bool IsValid => this.Errors.Count == 0;
    }
}