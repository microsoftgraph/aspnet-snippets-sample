// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TimeZoneConverter;

namespace SnippetsApp.TagHelpers
{
    // This tag helper formats a DateTimeOffset using
    // the specified time zone and format
    // Usage:
    // <date-time-offset value="" time-zone="" format=""></date-time-offset>
    public class DateTimeOffsetTagHelper : TagHelper
    {
        public DateTimeOffset? Value { get; set; }
        public string TimeZone { get; set; }
        public string Format { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Format))
            {
                // Handle empty format
                // Default to
                Format = "g";
            }

            DateTime dateTime;
            try
            {
                // TZConvert handles either an IANA or Windows identifier
                // Graph can return either
                var userTimeZone = TZConvert.GetTimeZoneInfo(TimeZone);
                dateTime = TimeZoneInfo.ConvertTimeFromUtc(Value.GetValueOrDefault().UtcDateTime, userTimeZone);
            }
            catch(TimeZoneNotFoundException)
            {
                // If the time zone isn't found, just use
                dateTime = Value.GetValueOrDefault().UtcDateTime;
            }

            output.TagName = "span";
            output.Content.SetContent(dateTime.ToString(Format));
        }
    }
}
