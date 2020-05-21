// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace SnippetsApp
{
    public static class GraphConstants
    {
        // Defines the permission scopes used by the app
        public readonly static string[] DefaultScopes =
        {
            UserReadWrite,
            MailboxSettingsRead
        };

        // Default page size for collections
        public const int PageSize = 25;

        // User
        public const string UserRead = "User.Read";
        public const string UserReadBasicAll = "User.ReadBasic.All";
        public const string UserReadAll = "User.Read.All";
        public const string UserReadWrite = "User.ReadWrite";
        public const string UserReadWriteAll = "User.ReadWrite.All";

        // Group
        public const string GroupReadWriteAll = "Group.ReadWrite.All";

        // Mailbox settings
        public const string MailboxSettingsRead = "MailboxSettings.Read";

        // Mail
        public const string MailRead = "Mail.Read";
        public const string MailReadWrite = "Mail.ReadWrite";
        public const string MailSend = "Mail.Send";

        // Calendar
        public const string CalendarReadWrite = "Calendars.ReadWrite";

        // Errors
        public const string ItemNotFound = "ErrorItemNotFound";
        public const string RequestDenied = "Authorization_RequestDenied";
        public const string RequestResourceNotFound = "Request_ResourceNotFound";
        public const string ResourceNotFound = "ResourceNotFound";
    }
}
