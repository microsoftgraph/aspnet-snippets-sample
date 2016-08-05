/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Resources;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        public ActionResult Index()
        {
            return View("Events");
        }

        // Get events.
        public async Task<ActionResult> GetMyEvents()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

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
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });

            }
            return View("Events", results);
        }

        // Create an event.
        public async Task<ActionResult> CreateEvent()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            string guid = Guid.NewGuid().ToString();
    
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

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
                DateTimeTimeZone startTime = new DateTimeTimeZone
                {
                    DateTime = new DateTime(2016, 12, 1, 9, 30, 0).ToString("o"),
                    TimeZone = TimeZoneInfo.Local.Id
                };
                DateTimeTimeZone endTime = new DateTimeTimeZone
                {
                    DateTime = new DateTime(2016, 12, 1, 10, 0, 0).ToString("o"),
                    TimeZone = TimeZoneInfo.Local.Id
                };

                // Event location
                var location = new Location
                {
                    DisplayName = Resource.Location_DisplayName,
                };

               // Add the event.
               var createdEvent = await graphClient.Me.Events.Request().AddAsync(new Event
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
                    item.Display = createdEvent.Subject;
                    item.Id = createdEvent.Id;
                    item.Properties.Add(Resource.Prop_Description, createdEvent.BodyPreview);
                    item.Properties.Add(Resource.Prop_Attendees, createdEvent.Attendees.Count());
                    item.Properties.Add(Resource.Prop_Start, createdEvent.Start.DateTime);
                    item.Properties.Add(Resource.Prop_End, createdEvent.End.DateTime);
                    item.Properties.Add(Resource.Prop_Id, createdEvent.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Get a specified event.
        public async Task<ActionResult> GetEvent(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                
                // Get the event.
                Event retrievedEvent = await graphClient.Me.Events[id].Request().GetAsync();
                
                if (retrievedEvent != null)
                {
                    
                    // Get event properties.
                    item.Display = retrievedEvent.Subject;
                    item.Id = retrievedEvent.Id;
                    item.Properties.Add(Resource.Prop_Description, retrievedEvent.BodyPreview);
                    item.Properties.Add(Resource.Prop_Attendees, retrievedEvent.Attendees.Count());
                    item.Properties.Add(Resource.Prop_Start, retrievedEvent.Start.DateTime);
                    item.Properties.Add(Resource.Prop_End, retrievedEvent.End.DateTime);
                    item.Properties.Add(Resource.Prop_Id, retrievedEvent.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Update an event. 
        // This snippets updates the event subject, time, and attendees.
        public async Task<ActionResult> UpdateEvent(string id, string subject)
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

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

                // Call the Microsoft Graph.
                Event updatedEvent = await graphClient.Me.Events[id].Request().UpdateAsync(new Event
                {
                    Subject = Resource.Updated + subject,
                    Attendees = attendees,
                    Start = startTime,
                    End = endTime
                });

                if (updatedEvent != null)
                {

                    // Get updated event properties.
                    item.Display = updatedEvent.Subject;
                    item.Id = updatedEvent.Id;
                    item.Properties.Add(Resource.Prop_Attendees, updatedEvent.Attendees.Count());
                    item.Properties.Add(Resource.Prop_Start, updatedEvent.Start.DateTime);
                    item.Properties.Add(Resource.Prop_End, updatedEvent.End.DateTime);
                    item.Properties.Add(Resource.Prop_Id, updatedEvent.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Delete an event.
        public async Task<ActionResult> DeleteEvent(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Delete the event.
                await graphClient.Me.Events[id].Request().DeleteAsync();
                
                // This operation doesn't return anything.
                item.Properties.Add(Resource.No_Return_Data, "");
                items.Add(item);
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }
    }
}