/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{
    public class EventsService
    {

        // Get events in all the current user's mail folders.
        public async Task<List<ResultsItem>> GetMyEvents(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get events.
            IUserEventsCollectionPage events = await graphClient.Me.Events.Request().GetAsync();

            if (events?.Count > 0)
            {
                foreach (Event current in events)
                {
                    items.Add(new ResultsItem
                    {
                        Display = current.Subject,
                        Id = current.Id
                    });
                }
            }
            return items;
        }

        // Get user's calendar view.
        // This snippets gets events for the next seven days.
        public async Task<List<ResultsItem>> GetMyCalendarView(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Define the time span for the calendar view.
            List<QueryOption> options = new List<QueryOption>();
            options.Add(new QueryOption("startDateTime", DateTime.Now.ToString("o")));
            options.Add(new QueryOption("endDateTime", DateTime.Now.AddDays(7).ToString("o")));

            ICalendarCalendarViewCollectionPage calendar = await graphClient.Me.Calendar.CalendarView.Request(options).GetAsync();

            if (calendar?.Count > 0)
            {
                foreach (Event current in calendar)
                {
                    items.Add(new ResultsItem
                    {
                        Display = current.Subject,
                        Id = current.Id
                    });
                }
            }
            return items;
        }

        // Create an event.
        // This snippet creates an hour-long event three days from now. 
        public async Task<List<ResultsItem>> CreateEvent(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            string guid = Guid.NewGuid().ToString();

            // List of attendees
            List<Attendee> attendees = new List<Attendee>();
            attendees.Add(new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Address = "mara@fabrikam.com"
                },
                Type = AttendeeType.Required
            });

            // Event body
            ItemBody body = new ItemBody
            {
                Content = Resource.Event + guid,
                ContentType = BodyType.Text
            };

            // Event start and end time
            // Another example date format: `new DateTime(2017, 12, 1, 9, 30, 0).ToString("o")`
            DateTimeTimeZone startTime = new DateTimeTimeZone
            {
                DateTime = DateTime.Now.AddDays(3).ToString("o"),
                TimeZone = TimeZoneInfo.Local.Id
            };
            DateTimeTimeZone endTime = new DateTimeTimeZone
            {
                DateTime = DateTime.Now.AddDays(3).AddHours(1).ToString("o"),
                TimeZone = TimeZoneInfo.Local.Id
            };

            // Event location
            Location location = new Location
            {
                DisplayName = Resource.Location_DisplayName,
            };

            // Add the event.
            Event createdEvent = await graphClient.Me.Events.Request().AddAsync(new Event
            {
                Subject = Resource.Event + guid.Substring(0, 8),
                Location = location,
                Attendees = attendees,
                Body = body,
                Start = startTime,
                End = endTime
            });

            if (createdEvent != null)
            {

                // Get event properties.
                items.Add(new ResultsItem
                {
                    Display = createdEvent.Subject,
                    Id = createdEvent.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Description, createdEvent.BodyPreview },
                        { Resource.Prop_Attendees, createdEvent.Attendees.Count() },
                        { Resource.Prop_Start, createdEvent.Start.DateTime },
                        { Resource.Prop_End, createdEvent.End.DateTime },
                        { Resource.Prop_Id, createdEvent.Id }
                    }
                });
            }
            return items;
        }

        // Get a specified event.
        public async Task<List<ResultsItem>> GetEvent(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the event.
            Event retrievedEvent = await graphClient.Me.Events [id].Request().GetAsync();
                
            if (retrievedEvent != null)
            {

                // Get event properties.
                items.Add(new ResultsItem
                {
                    Display = retrievedEvent.Subject,
                    Id = retrievedEvent.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Description, retrievedEvent.BodyPreview },
                        { Resource.Prop_Attendees, retrievedEvent.Attendees.Count() },
                        { Resource.Prop_Start, retrievedEvent.Start.DateTime },
                        { Resource.Prop_End, retrievedEvent.End.DateTime },
                        { Resource.Prop_ResponseStatus, retrievedEvent.ResponseStatus.Response },
                        { Resource.Prop_Id, retrievedEvent.Id }
                    }
                });
            }
            return items;
        }


        // Update an event. 
        // This snippets updates the event subject, time, and attendees.
        public async Task<List<ResultsItem>> UpdateEvent(GraphServiceClient graphClient, string id, string name)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // New start and end time.
            DateTimeTimeZone startTime = new DateTimeTimeZone
            {
                DateTime = new DateTime(2016, 12, 1, 13, 0, 0).ToString("o"),
                TimeZone = TimeZoneInfo.Local.Id
            };
            DateTimeTimeZone endTime = new DateTimeTimeZone
            {
                DateTime = new DateTime(2016, 12, 1, 14, 0, 0).ToString("o"),
                TimeZone = TimeZoneInfo.Local.Id
            };

            // Get the current list of attendees, and then add an attendee.
            Event originalEvent = await graphClient.Me.Events[id].Request().Select("attendees").GetAsync();
            List<Attendee> attendees = originalEvent.Attendees as List<Attendee>;
            attendees.Add(new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Address = "aziz@fabrikam.com"
                },
                Type = AttendeeType.Required
            });

            // Update the event.
            Event updatedEvent = await graphClient.Me.Events[id].Request().UpdateAsync(new Event
            {
                Subject = Resource.Updated + name,
                Attendees = attendees,
                Start = startTime,
                End = endTime
            });

            if (updatedEvent != null)
            {

                // Get updated event properties.
                items.Add(new ResultsItem
                {
                    Display = updatedEvent.Subject,
                    Id = updatedEvent.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Attendees, updatedEvent.Attendees.Count() },
                        { Resource.Prop_Start, updatedEvent.Start.DateTime },
                        { Resource.Prop_End, updatedEvent.End.DateTime },
                        { Resource.Prop_Id, updatedEvent.Id }
                    }
                });
            }
            return items;
        }

        // Delete a specified event.
        public async Task<List<ResultsItem>> DeleteEvent(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Delete the event.
            await graphClient.Me.Events[id].Request().DeleteAsync();

            items.Add(new ResultsItem
            {

                // This operation doesn't return anything.
                Properties = new Dictionary<string, object>
                {
                    { Resource.No_Return_Data, "" }
                }
            });
            return items;
        }

        // Accept a meeting request.
        public async Task<List<ResultsItem>> AcceptMeetingRequest(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // This snippet first checks whether the selected event originates with an invitation from the current user. If it did, 
            // the SDK would throw an ErrorInvalidRequest exception because organizers can't accept their own invitations.
            Event myEvent = await graphClient.Me.Events[id].Request().Select("ResponseStatus").GetAsync();
            if (myEvent.ResponseStatus.Response != ResponseType.Organizer)
            {

                // Accept the meeting.
                await graphClient.Me.Events[id].Accept(Resource.GenericText).Request().PostAsync();

                items.Add(new ResultsItem
                {

                    // This operation doesn't return anything.
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.No_Return_Data, "" }
                    }
                });
            }
            else
            {
                items.Add(new ResultsItem
                {

                    // Let the user know the operation isn't supported for this event.
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Event_CannotAcceptOwnMeeting, "" }
                    }
                });
            }
            return items;
        }
    }
}