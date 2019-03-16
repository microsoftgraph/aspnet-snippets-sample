/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using Resources;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        // Initialize the GraphServiceClient.
        GraphServiceClient graphClient;
        EventsService eventsService;
        
        public EventsController()
        {
            // Initialize the GraphServiceClient.
            graphClient = SDKHelper.GetAuthenticatedClient();
            eventsService = new EventsService();
        }
        public ActionResult Index()
        {
            return View("Events");
        }

        // Get events.
        public async Task<ActionResult> GetMyEvents()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                // Get events.
                results.Items = await eventsService.GetMyEvents(graphClient);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Get user's calendar view.
        public async Task<ActionResult> GetMyCalendarView()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                // Get a calendar view.
                results.Items = await eventsService.GetMyCalendarView(graphClient);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Create an event.
        // This snippet creates an hour-long event three days from now. 
        public async Task<ActionResult> CreateEvent()
        {
            ResultsViewModel results = new ResultsViewModel();    
            try
            {
                // Create the event.
                results.Items = await eventsService.CreateEvent(graphClient);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Get a specified event.
        public async Task<ActionResult> GetEvent(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                // Get the event.
                results.Items = await eventsService.GetEvent(graphClient, id);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Update an event. 
        // This snippets updates the event subject, time, and attendees.
        public async Task<ActionResult> UpdateEvent(string id, string name)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                // Update the event.
                results.Items = await eventsService.UpdateEvent(graphClient, id, name);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Delete an event.
        public async Task<ActionResult> DeleteEvent(string id)
        {
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {
                // Delete the event.
                results.Items = await eventsService.DeleteEvent(graphClient, id);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }

        // Accept a meeting request.
        // If the current user is the organizer of the meeting, the snippet will not work since organizers can't accept their
        // own invitations.
        public async Task<ActionResult> AcceptMeetingRequest(string id)
        {
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {
                // Accept the meeting.
                results.Items = await eventsService.AcceptMeetingRequest(graphClient, id);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Events", results);
        }
    }
}