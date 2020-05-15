// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

// <WithAlertResultSnippet>
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SnippetsApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnippetsApp
{
    // WithAlertResult adds temporary error/info/success
    // messages to the result of a controller action.
    // This data is read and displayed by the _AlertPartial view
    public class WithAlertResult : IActionResult
    {
        public IActionResult Result { get; }

        public Alert Alert { get; }

        public WithAlertResult(IActionResult result,
                               string type,
                               string message,
                               string debugInfo,
                               string actionText,
                               string actionUrl)
        {
            Result = result;
            Alert = new Alert
            {
                Type = type,
                Message = message,
                DebugInfo = debugInfo,
                ActionText = actionText,
                ActionUrl = actionUrl,
            };
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices
            .GetService<ITempDataDictionaryFactory>();

            var tempData = factory.GetTempData(context.HttpContext);

            tempData.AddAlert("_alertData", Alert);

            await Result.ExecuteResultAsync(context);
        }
    }
}
// </WithAlertResultSnippet>
