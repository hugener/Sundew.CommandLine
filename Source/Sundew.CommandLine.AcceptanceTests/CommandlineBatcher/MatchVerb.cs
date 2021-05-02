﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatchVerb.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.CommandLine.AcceptanceTests.CommandlineBatcher
{
    using System.Collections.Generic;
    using Sundew.CommandLine;

    public class MatchVerb : IVerb
    {
        private readonly List<string> patterns;

        public MatchVerb()
         : this(new List<string>(), null!, false, '|', ',', null, null)
        {
        }

        public MatchVerb(List<string> patterns, string? input, bool useStandardInput, char batchSeparator, char batchValueSeparator, string? format, string? outputPath, bool merge = false)
        {
            this.patterns = patterns;
            this.BatchSeparator = batchSeparator;
            this.BatchValueSeparator = batchValueSeparator;
            this.Input = input;
            this.UseStandardInput = useStandardInput;
            this.Format = format;
            this.OutputPath = outputPath;
            this.Merge = merge;
        }

        public IReadOnlyList<string> Patterns => this.patterns;

        public string? Input { get; private set; }

        public bool UseStandardInput { get; private set; }

        public string? Format { get; private set; }

        public char BatchSeparator { get; private set; }

        public char BatchValueSeparator { get; private set; }

        public string? OutputPath { get; private set; }

        public bool Merge { get; private set; }

        public Verbosity Verbosity { get; private set; }

        public IVerb? NextVerb { get; } = null;

        public string Name { get; } = "match";

        public string? ShortName { get; } = "m";

        public string HelpText { get; } = "Matches the specified input to patterns and maps it to key-value pairs.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "It's a multi line string.")]
        public void Configure(IArgumentsBuilder argumentsBuilder)
        {
            argumentsBuilder.AddRequiredList(
                "p",
                "patterns",
                this.patterns,
                @"The patterns (Regex) to be matched in the order they are specified
Format: {pattern} => {batch}[,batch]*
Batches may consist of multiple values, separated by the value-separator
Batches can also contain regex group names in the format {group-name}",
                true);
            argumentsBuilder.AddOptional("f", "format", () => this.Format, s => this.Format = s, "The format to apply to each key value pair.");
            argumentsBuilder.AddOptional("bs", "batch-separator", () => this.BatchSeparator.ToString(), s => this.BatchSeparator = s[0], "The character use to split batches.");
            argumentsBuilder.AddOptional("bvs", "batch-value-separator", () => this.BatchValueSeparator.ToString(), s => this.BatchValueSeparator = s[0], "The character use to split batch values.");
            argumentsBuilder.AddSwitch("m", "merge", this.Merge, b => this.Merge = b, "Indicates whether outputs should be merged and output as one");
            argumentsBuilder.AddOptionalEnum("lv", "logging-verbosity", () => this.Verbosity, v => this.Verbosity = v, "Logging verbosity: {0}");
            argumentsBuilder.RequireAnyOf("Input", builder => builder
               .Add("i", "input", () => this.Input, s => this.Input = s, "The input to be matched", true)
               .AddSwitch(null, "input-stdin", this.UseStandardInput, b => this.UseStandardInput = b, "Indicates that the input should be read from standard input"));
            argumentsBuilder.AddOptionalValue("output-path", () => this.OutputPath, s => this.OutputPath = s, "The output path, if not specified application will output to stdout");
        }
    }
}
