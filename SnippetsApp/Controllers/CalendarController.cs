// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using SnippetsApp.Models;
using TimeZoneConverter;

namespace SnippetsApp.Controllers
{
    [AuthorizeForScopes(Scopes = new [] { GraphConstants.CalendarReadWrite })]
    public class CalendarController : BaseController
    {
        private readonly string[] _calendarScopes =
            new [] { GraphConstants.CalendarReadWrite };

        public CalendarController(
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger) : base(tokenAcquisition, logger)
        {
        }

        // GET /Calendar
        // Displays a calendar view of the current week for
        // the logged-in user
        public async Task<IActionResult> Index()
        {
            try
            {
                // TZConvert handles either an IANA or Windows identifier
                // Graph can return either
                var userTimeZone = TZConvert.GetTimeZoneInfo(
                    User.GetUserGraphTimeZone());
                var startOfWeek = CalendarController.GetUtcStartOfWeekInTimeZone(
                    DateTime.Today, userTimeZone);

                var events = await GetUserWeekCalendar(startOfWeek);

                var model = new CalendarViewModel(startOfWeek, events);

                return View(model);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return View(new CalendarViewModel())
                    .WithError("Error getting calendar view", ex.Message);
            }
        }

        // GET /Calendar/Display?eventId=""
        // eventId: ID of the event to display
        // Displays the requested event allowing the user
        // to delete, update, or respond
        public async Task<IActionResult> Display(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return RedirectToAction("Index")
                    .WithError("Event ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_calendarScopes);

                // GET /me/events/eventId
                var graphEvent = await graphClient.Me
                    .Events[eventId]
                    .Request()
                    // Send the Prefer header so times are in the user's timezone
                    .Header("Prefer", $"outlook.timezone=\"{User.GetUserGraphTimeZone()}\"")
                    // Request only the fields used by the app
                    .Select(e => new
                    {
                        e.Attendees,
                        e.Body,
                        e.End,
                        e.Id,
                        e.IsOrganizer,
                        e.Location,
                        e.Organizer,
                        e.ResponseStatus,
                        e.Start,
                        e.Subject
                    })
                    // Include attachments in the response
                    .Expand("attachments")
                    .GetAsync();

                return View(graphEvent);
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index")
                    .WithError($"Error getting event with ID {eventId}",
                        ex.Error.Message);
            }
        }

        // GET /Calendar/New
        // Gets the new event form
        public IActionResult New()
        {
            return View();
        }

        // POST /Calendar/New
        // Receives form data to create a new event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New([Bind("Subject,Attendees,Start,End,Body")] NewEvent newEvent)
        {
            var timeZone = User.GetUserGraphTimeZone();

            // Create a Graph event with the required fields
            var graphEvent = new Event
            {
                Subject = newEvent.Subject,
                Start = new DateTimeTimeZone
                {
                    DateTime = newEvent.Start.ToString("o"),
                    // Use the user's time zone
                    TimeZone = timeZone
                },
                End = new DateTimeTimeZone
                {
                    DateTime = newEvent.End.ToString("o"),
                    // Use the user's time zone
                    TimeZone = timeZone
                }
            };

            // Add body if present
            if (!string.IsNullOrEmpty(newEvent.Body))
            {
                graphEvent.Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = newEvent.Body
                };
            }

            // Add attendees if present
            if (!string.IsNullOrEmpty(newEvent.Attendees))
            {
                var attendees =
                    newEvent.Attendees.Split(';', StringSplitOptions.RemoveEmptyEntries);

                if (attendees.Length > 0)
                {
                    var attendeeList = new List<Attendee>();
                    foreach (var attendee in attendees)
                    {
                        attendeeList.Add(new Attendee{
                            EmailAddress = new EmailAddress
                            {
                                Address = attendee
                            },
                            Type = AttendeeType.Required
                        });
                    }
                }
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_calendarScopes);
                // Add the event
                // POST /me/events
                await graphClient.Me.Events
                    .Request()
                    .AddAsync(graphEvent);

                // Redirect to the calendar view with a success message
                return RedirectToAction("Index").WithSuccess("Event created");
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                // Redirect to the calendar view with an error message
                return RedirectToAction("Index")
                    .WithError("Error creating event", ex.Error.Message);
            }
        }

        // POST /Calendar/Update
        // Receives form data to update the start and end times
        // for an event
        // eventId: ID of the event to update
        // startTime: New start time
        // startTimeZone: Time zone for start time
        // endTime: New end time
        // endTimeZone: Time zone for end time
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string eventId,
                                                string startTime,
                                                string startTimeZone,
                                                string endTime,
                                                string endTimeZone)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return RedirectToAction("Index")
                    .WithError("Event ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_calendarScopes);

                // Create a new Event object with only the
                // fields to update set
                var updateEvent = new Event
                {
                    Start = new DateTimeTimeZone
                    {
                        DateTime = startTime,
                        TimeZone = startTimeZone
                    },
                    End = new DateTimeTimeZone
                    {
                        DateTime = endTime,
                        TimeZone = endTimeZone
                    }
                };

                // PATCH /me/events/eventId
                await graphClient.Me
                    .Events[eventId]
                    .Request()
                    .UpdateAsync(updateEvent);

                return RedirectToAction("Display", new { eventId = eventId })
                .WithSuccess("Event times updated");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { eventId = eventId })
                    .WithError($"Error updating event with ID {eventId}",
                        ex.Error.Message);
            }

        }

        // POST /Calendar/Accept
        // Receives form data to accept an event
        // eventId: ID of the event to accept
        // sendResponse: True to send the response to the organizer
        // comment: Optional message to include in response to organizer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(string eventId,
                                                bool sendResponse,
                                                string comment)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return RedirectToAction("Index")
                    .WithError("Event ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_calendarScopes);

                // POST /me/events/eventId/accept
                await graphClient.Me
                    .Events[eventId]
                    .Accept(comment, sendResponse)
                    .Request()
                    .PostAsync();

                return RedirectToAction("Display", new { eventId = eventId })
                    .WithSuccess("Meeting accepted");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { eventId = eventId })
                    .WithError("Error accepting meeting",
                        ex.Error.Message);
            }
        }

        // POST /Calendar/Tentative
        // Receives form data to tentatively accept an event
        // eventId: ID of the event to tentatively accept
        // sendResponse: True to send the response to the organizer
        // comment: Optional message to include in response to organizer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tentative(string eventId,
                                                   bool sendResponse,
                                                   string comment)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return RedirectToAction("Index")
                    .WithError("Event ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_calendarScopes);

                // POST /me/events/eventId/tentativelyAccept
                await graphClient.Me
                    .Events[eventId]
                    .TentativelyAccept(comment, sendResponse)
                    .Request()
                    .PostAsync();

                return RedirectToAction("Display", new { eventId = eventId })
                    .WithSuccess("Meeting tentatively accepted");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { eventId = eventId })
                    .WithError("Error tentatively accepting meeting",
                        ex.Error.Message);
            }
        }

        // POST /Calendar/Decline
        // Receives form data to decline an event
        // eventId: ID of the event to decline
        // sendResponse: True to send the response to the organizer
        // comment: Optional message to include in response to organizer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(string eventId,
                                                 bool sendResponse,
                                                 string comment)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return RedirectToAction("Index")
                    .WithError("Event ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_calendarScopes);

                // POST /me/events/eventId/decline
                await graphClient.Me
                    .Events[eventId]
                    .Decline(comment, sendResponse)
                    .Request()
                    .PostAsync();

                return RedirectToAction("Index")
                    .WithSuccess("Meeting declined");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { eventId = eventId })
                    .WithError("Error declining meeting",
                        ex.Error.Message);
            }
        }

        // POST /Calendar/Delete
        // Deletes an event from the calendar
        // If user is the organizer and there are attendees,
        // attendees will receive a cancellation
        // eventId: ID of the event to delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return RedirectToAction("Index")
                    .WithError("Event ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_calendarScopes);

                // DELETE /me/events/eventId
                await graphClient.Me
                    .Events[eventId]
                    .Request()
                    .DeleteAsync();

                return RedirectToAction("Index")
                    .WithSuccess("Event deleted");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index")
                    .WithError($"Error deleting event with ID {eventId}",
                        ex.Error.Message);
            }
        }

        private async Task<IList<Event>> GetUserWeekCalendar(DateTime startOfWeek)
        {
            var graphClient = GetGraphClientForScopes(_calendarScopes);

            // Configure a calendar view for the current week
            var endOfWeek = startOfWeek.AddDays(7);

            var viewOptions = new List<QueryOption>
            {
                new QueryOption("startDateTime", startOfWeek.ToString("o")),
                new QueryOption("endDateTime", endOfWeek.ToString("o"))
            };

            var events = await graphClient.Me
                .CalendarView
                .Request(viewOptions)
                // Send user time zone in request so date/time in
                // response will be in preferred time zone
                .Header("Prefer", $"outlook.timezone=\"{User.GetUserGraphTimeZone()}\"")
                // Get max 50 per request
                .Top(50)
                // Only return fields app will use
                .Select(e => new
                {
                    e.Subject,
                    e.Organizer,
                    e.Start,
                    e.End
                })
                // Order results chronologically
                .OrderBy("start/dateTime")
                .GetAsync();

            IList<Event> allEvents;
            // Handle case where there are more than 50
            if (events.NextPageRequest != null)
            {
                allEvents = new List<Event>();
                // Create a page iterator to iterate over subsequent pages
                // of results. Build a list from the results
                var pageIterator = PageIterator<Event>.CreatePageIterator(
                    graphClient, events,
                    (e) => {
                        allEvents.Add(e);
                        return true;
                    }
                );
                await pageIterator.IterateAsync();
            }
            else
            {
                // If only one page, just use the result
                allEvents = events.CurrentPage;
            }

            return allEvents;
        }

        private static DateTime GetUtcStartOfWeekInTimeZone(DateTime today, TimeZoneInfo timeZone)
        {
            // Assumes Sunday as first day of week
            int diff = System.DayOfWeek.Sunday - today.DayOfWeek;

            // create date as unspecified kind
            var unspecifiedStart = DateTime.SpecifyKind(today.AddDays(diff), DateTimeKind.Unspecified);

            // convert to UTC
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedStart, timeZone);
        }
    }
}
