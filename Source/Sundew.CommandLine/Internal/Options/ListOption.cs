﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListOption.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.CommandLine.Internal.Options
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Sundew.Base.Collections;
    using Sundew.Base.Computation;
    using Sundew.CommandLine.Internal.Helpers;

    internal class ListOption<TValue> : IOption, IListSerializationInfo<TValue>
    {
        private readonly Deserialize<TValue> deserialize;

        public ListOption(
            string? name,
            string alias,
            IList<TValue> list,
            Serialize<TValue> serialize,
            Deserialize<TValue> deserialize,
            bool isRequired,
            string helpText,
            bool useDoubleQuotes)
        {
            this.Name = name;
            this.Alias = alias;
            this.List = list;
            this.DefaultList = this.List.ToList();
            this.Serialize = serialize;
            this.deserialize = deserialize;
            this.UseDoubleQuotes = useDoubleQuotes;
            this.IsRequired = isRequired;
            this.HelpText = helpText;
            this.Usage = HelpTextHelper.GetUsage(name, alias);
        }

        public string? Name { get; }

        public string Alias { get; }

        public Separators Separators => default;

        public IList<TValue> List { get; }

        public IList<TValue> DefaultList { get; }

        public Serialize<TValue> Serialize { get; }

        public bool UseDoubleQuotes { get; }

        public bool IsRequired { get; }

        public string Usage { get; }

        public string HelpText { get; }

        public bool IsNesting => false;

        public Result<bool, GeneratorError> SerializeTo(StringBuilder stringBuilder, Settings settings, bool useAliases)
        {
            var wasSerialized = SerializationHelper.SerializeTo(this, this.List, stringBuilder, settings, () =>
            {
                SerializationHelper.AppendNameOrAlias(stringBuilder, this.Name, this.Alias, useAliases);
                stringBuilder.Append(Constants.SpaceCharacter);
            });
            if (!wasSerialized && this.IsRequired)
            {
                return Result.Error(new GeneratorError(this, GeneratorErrorType.RequiredOptionMissing));
            }

            return Result.Success(wasSerialized);
        }

        public Result.IfError<ParserError> DeserializeFrom(
            CommandLineArgumentsParser commandLineArgumentsParser,
            ArgumentList argumentList,
            ReadOnlySpan<char> value,
            Settings settings)
        {
            this.List.Clear();
            var currentResult = this.DeserializeFrom(value, settings);
            if (argumentList.TryMoveNext(out _))
            {
                foreach (var argument in argumentList)
                {
                    if (argument[0] == Constants.ArgumentStartCharacter)
                    {
                        argumentList.MoveBack();
                        break;
                    }

                    currentResult = this.DeserializeFrom(CommandLineArgumentsParser.RemoveValueEscapeIfNeeded(argument.AsSpan()), settings);
                }
            }

            return currentResult;
        }

        public void AppendHelpText(StringBuilder stringBuilder, Settings settings, int maxName, int maxAlias, int maxHelpText, int indent, bool isForVerb)
        {
            HelpTextHelper.AppendHelpText(stringBuilder, settings, this, maxName, maxAlias, maxHelpText, indent, isForVerb);
        }

        public void ResetToDefault(CultureInfo cultureInfo)
        {
            this.List.Clear();
            this.List.AddRange(this.DefaultList);
        }

        public void AppendDefaultText(StringBuilder stringBuilder, Settings settings, bool isNested)
        {
            if (this.IsRequired)
            {
                stringBuilder.AppendLine(Constants.RequiredText);
                return;
            }

            stringBuilder.Append(Constants.DefaultText);
            if (!SerializationHelper.SerializeTo(this, this.DefaultList, stringBuilder, settings, null))
            {
                stringBuilder.AppendLine(Constants.NoneText);
            }
        }

        private Result.IfError<ParserError> DeserializeFrom(ReadOnlySpan<char> value, Settings settings)
        {
            SerializationHelper.DeserializeTo(this.List, this.deserialize, value, settings);
            return Result.Success();
        }
    }
}