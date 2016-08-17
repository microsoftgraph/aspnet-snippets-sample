/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using Resources;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{

    [Authorize]
    public class FilesController : Controller
    {
        FilesService filesService = new FilesService();

        public ActionResult Index()
        {
            return View("Files");
        }

        // Get the drive items in the root directory of the current user's default drive.
        public async Task<ActionResult> GetMyFilesAndFolders()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the files and folders in the current user's drive.
                results.Items = await filesService.GetMyFilesAndFolders(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }

        // Get the items that are shared with the current user.
        public async Task<ActionResult> GetSharedWithMe()
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the shared items.
                results.Items = await filesService.GetSharedWithMe(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });

            }
            return View("Files", results);
        }

        // Get the current user's default drive.
        public async Task<ActionResult> GetMyDrive()
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the current user's default drive.
                results.Items = await filesService.GetMyDrive(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }

        // Create a text file in the current user's root directory.
        public async Task<ActionResult> CreateFile()
        {
            ResultsViewModel results = new ResultsViewModel();
                                
            try
                {

                    // Initialize the GraphServiceClient.
                    GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                    // Add the file.
                    results.Items = await filesService.CreateFile(graphClient);
                }
                catch (ServiceException se)
                {
                    if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                    return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
                }
            return View("Files", results);
        }

        // Create a folder in the current user's root directory. 
        public async Task<ActionResult> CreateFolder()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Add the folder.
                results.Items = await filesService.CreateFolder(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }

        // Get a file or folder (metadata) in the current user's drive.
        public async Task<ActionResult> GetFileOrFolderMetadata(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the file or folder object.
                results.Items = await filesService.GetFileOrFolderMetadata(graphClient, id);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }

        // Download the content of an existing file.
        // This snippet returns the length of the file stream and some file metadata properties.
        public async Task<ActionResult> DownloadFile(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Download the file.
                results.Items = await filesService.DownloadFile(graphClient, id);

                // Handle selected item is not supported.
                foreach (var item in results.Items)
                {
                    if (item.Properties.ContainsKey(Resource.File_ChooseFile)) results.Selectable = false;
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }

        // Update the metadata of a file or folder. 
        // This snippet updates the item's name. 
        // To move an item, point the ParentReference.Id or ParentReference.Path property to the target destination.
        public async Task<ActionResult> UpdateFileOrFolderMetadata(string id, string name)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Update the item.
                results.Items = await filesService.UpdateFileOrFolderMetadata(graphClient, id, name);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }

        // Update the content of a file in the user's root directory. 
        // This snippet replaces the text content of a .txt file.
        public async Task<ActionResult> UpdateFileContent(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the file. Make sure it's a .txt file (for the purposes of this snippet).
                results.Items = await filesService.UpdateFileContent(graphClient, id);

                // Handle selected item is not supported.
                foreach (var item in results.Items)
                {
                    if (item.Properties.ContainsKey(Resource.File_ChooseTextFile)) results.Selectable = false;
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }

        // Delete a file in the user's root directory.
        public async Task<ActionResult> DeleteFileOrFolder(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Delete the item.
                results.Items = await filesService.DeleteFileOrFolder(graphClient, id);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Files", results);
        }
    }
}
