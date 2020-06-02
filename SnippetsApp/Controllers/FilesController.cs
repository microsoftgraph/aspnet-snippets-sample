// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using SnippetsApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnippetsApp.Controllers
{
    public class FilesController : BaseController
    {
      private readonly string[] _filesScopes =
            new [] { GraphConstants.FilesReadWrite };

        public FilesController(
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger) : base(tokenAcquisition, logger)
        {
        }

        // GET /Files?$folderId=""
        // folderId: ID of the selected folder
        // Displays the files and folders in a specific folder
        // If no folderId is given, displays contents of the root
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Index(string folderId = null)
        {
            return await GetViewForFolder(folderId);
        }

        // GET /Files/Page?pageUrl=""&folderId=""
        // pageUrl: The page URL to get the next page of results
        // folderId: ID of the currently selected folder
        // Gets the next page of results when a message list is paged
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Page(string pageUrl,
                                              string folderId = null)
        {
            return await GetViewForFolder(folderId, pageUrl);
        }

        // GET /Files/SharedWithMe?pageUrl=""
        // pageUrl: The page URL to get the next page of results, absent for the first page
        // Gets a page of items shared with the user
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.FilesReadWriteAll })]
        public async Task<IActionResult> SharedWithMe(string pageUrl = null)
        {
            var model = new FilesViewDisplayModel();

            var scopes = new[] { GraphConstants.FilesReadWriteAll };
            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                IDriveSharedWithMeRequest sharedItemsRequest;

                // Is this a page request?
                if (string.IsNullOrEmpty(pageUrl))
                {
                    // Not a page request, use the Graph client
                    // to build the request
                    // GET /me/drive/sharedWithMe
                    sharedItemsRequest = graphClient.Me
                        .Drive
                        .SharedWithMe()
                        .Request();
                }
                else
                {
                    // This is a page request, so instead of using
                    // the request builders, initialize the request directly
                    // from the URL
                    sharedItemsRequest = new DriveSharedWithMeRequest(
                        pageUrl, graphClient, null);
                }

                // Send the request
                var sharedItemPage = await sharedItemsRequest.GetAsync();

                // Results can include folders and/or files, so
                // go through them and add them to the appropriate list
                model.Files = new List<DriveItem>();
                model.Folders = new List<DriveItem>();

                foreach (var item in sharedItemPage.CurrentPage)
                {
                    if (item.RemoteItem.Folder != null)
                    {
                        model.Folders.Add(item);
                    }
                    else if (item.RemoteItem.File != null)
                    {
                        model.Files.Add(item);
                    }
                }

                model.NextPageUrl = sharedItemPage.NextPageRequest?
                    .GetHttpRequestMessage().RequestUri.ToString();

                return View(model);
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Error", "Home")
                    .WithError("Error getting shared items",
                        ex.Error.Message);
            }
        }

        // GET /Files/Drive
        // Gets properties of the user's drive
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Drive()
        {
            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // GET /me/drive
                var drive = await graphClient.Me
                    .Drive
                    .Request()
                    .GetAsync();

                return View(drive);
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Error", "Home")
                    .WithError("Error getting drive",
                        ex.Error.Message);
            }
        }

        // Get /Files/Display?fileId=""
        // fileId: ID of the file to display
        // Displays details of the requested file allowing user
        // to share, delete, etc.
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Display(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("File ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // Get /me/drive/items/fileId
                var file = await graphClient.Me
                    .Drive
                    .Items[fileId]
                    .Request()
                    // Only request the fields used by the app
                    .Select(d => new
                    {
                        d.CreatedBy,
                        d.CreatedDateTime,
                        d.Id,
                        d.LastModifiedBy,
                        d.LastModifiedDateTime,
                        d.Name,
                        d.ParentReference,
                        d.Size
                    })
                    .GetAsync();

                return View(file);
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index")
                    .WithError($"Error getting file with ID {fileId}",
                        ex.Error.Message);
            }
        }

        // POST /Files/Download
        // fileId: The ID of the file to download
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Download(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("File ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // Get the file name and type
                // GET /me/drive/items/fileId
                var file = await graphClient.Me
                    .Drive
                    .Items[fileId]
                    .Request()
                    .Select(f => new { f.File, f.Name })
                    .GetAsync();

                if (file.File == null)
                {
                    return RedirectToAction("Error", "Home")
                    .WithError("Invalid file ID");
                }

                // Get the contents of the file
                // GET /me/drive/items/fileId/content
                var fileContents = await graphClient.Me
                    .Drive
                    .Items[fileId]
                    .Content
                    .Request()
                    .GetAsync();

                // Return a file stream using the file name, MIME type, and
                // contents
                return new FileStreamResult(fileContents, file.File.MimeType)
                {
                    FileDownloadName = file.Name
                };
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { fileId = fileId })
                    .WithError($"Error downloading file",
                        ex.Error.Message);
            }
        }

        // POST /Files/Share
        // fileId: The ID of the file to share
        // Gets a sharing link for the specified file
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Share(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("File ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // Get a read-only link to the file
                // POST /me/drive/items/fileId/createLink
                var permission = await graphClient.Me
                    .Drive
                    .Items[fileId]
                    .CreateLink("view")
                    .Request()
                    .PostAsync();

                // Redirect back to the display of the file, showing the
                // sharing link in an info box
                return RedirectToAction("Display", new { fileId = fileId })
                    .WithInfo("Send the link below to share this file", permission.Link.WebUrl);

            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { fileId = fileId })
                    .WithError($"Error downloading file",
                        ex.Error.Message);
            }
        }

        // POST /Files/Update
        // fileId: ID of the file to update
        // newFileName: New file name
        // Updates the name of a file
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Update(string fileId, string newFileName)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("Message ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // Create a new DriveItem object with just the properties
                // to update, in this case, Name
                var updateFile = new DriveItem
                {
                    Name = newFileName
                };

                // PATCH /me/drive/items/fileId
                //
                // {
                //   "name": "..."
                // }
                await graphClient.Me
                    .Drive
                    .Items[fileId]
                    .Request()
                    .UpdateAsync(updateFile);

                return RedirectToAction("Display", new { fileId = fileId })
                    .WithSuccess("File name updated");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { fileId = fileId })
                    .WithError("Error updating file name", ex.Error.Message);
            }
        }

        // POST /Files/Delete
        // fileId: ID of the message to delete
        // returnFolderId: ID of the folder to select in the view after delete
        // Deletes the specified file and redirects to
        // the Index view with return folder selected
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> Delete(string fileId,
                                                string returnFolderId = null)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("File ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // DELETE /me/drive/items/fileId
                await graphClient.Me
                    .Drive
                    .Items[fileId]
                    .Request()
                    .DeleteAsync();

                return RedirectToAction("Index", new { folderId = returnFolderId })
                    .WithSuccess("File deleted");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { fileId = fileId })
                    .WithError($"Error deleting message",
                        ex.Error.Message);
            }
        }

        // GET /Files/NewFile
        // Get the form to create a new file
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public IActionResult NewFile(string folderId)
        {
            return View(model: folderId);
        }

        // POST /Files/NewFile
        // Creates a new file via PUT
        // Only use for files < 4MB
        // folderId: The ID of the folder to create the new file in
        // uploadFiles: The file posted by the form
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> NewFile(string folderId,
                                                 List<IFormFile> uploadFiles)
        {
            if (string.IsNullOrEmpty(folderId))
            {
                return RedirectToAction("NewFile")
                    .WithError("No folder ID was specified");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                var file = uploadFiles.First();

                // PUT /me/drive/items/folderId:/fileName:/content
                var newFile = await graphClient.Me
                    .Drive
                    .Items[folderId]
                    .ItemWithPath(file.FileName)
                    .Content
                    .Request()
                    .PutAsync<DriveItem>(file.OpenReadStream());

                return RedirectToAction("Display", new { fileId = newFile.Id })
                    .WithSuccess("File uploaded");
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index", new { folderId = folderId })
                    .WithError("Error uploading file", ex.Error.Message);
            }
        }

        // GET /Files/UploadFile
        // Gets the form to upload a file
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public IActionResult UploadFile(string folderId)
        {
            return View(model: folderId);
        }

        // POST /Files/UploadFile
        // Creates a new file via upload session
        // folderId: The ID of the folder to create the new file in
        // uploadFiles: The file posted by the form
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public async Task<IActionResult> UploadFile(string folderId,
                                                    List<IFormFile> uploadFiles)
        {
            if (string.IsNullOrEmpty(folderId))
            {
                return RedirectToAction("UploadFile")
                    .WithError("No folder ID was specified");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                var file = uploadFiles.First();

                // Use properties to specify the conflict behavior
                // in this case, replace
                var uploadProps = new DriveItemUploadableProperties
                {
                    ODataType = null,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "@microsoft.graph.conflictBehavior", "replace" }
                    }
                };

                // POST /me/drive/items/folderId:/fileName:/createUploadSession
                var uploadSession = await graphClient.Me
                    .Drive
                    .Items[folderId]
                    .ItemWithPath(file.FileName)
                    .CreateUploadSession(uploadProps)
                    .Request()
                    .PostAsync();

                // Max slice size must be a multiple of 320 KiB
                // This amount of bytes will be uploaded each iteration
                int maxSliceSize = 320 * 1024;
                var fileUploadTask =
                    new LargeFileUploadTask<DriveItem>(uploadSession, file.OpenReadStream(), maxSliceSize);

                // Create a callback that is invoked after each slice is uploaded
                IProgress<long> callback = new Progress<long>(progress => {
                    _logger.LogInformation($"Uploaded {progress} bytes of {file.Length} bytes");
                });

                // Start the upload
                var uploadResult = await fileUploadTask.UploadAsync(callback);

                if (uploadResult.UploadSucceeded)
                {
                    // On success, the new file is contained in the
                    // ItemResponse property of the result
                    return RedirectToAction("Display", new { fileId = uploadResult.ItemResponse.Id })
                        .WithSuccess("File uploaded");
                }

                return RedirectToAction("Index", new { folderId = folderId })
                    .WithError("Error uploading file");
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index", new { folderId = folderId })
                    .WithError("Error uploading file", ex.Error.Message);
            }
        }

        // GET /Files/NewFolder
        // Get the form to create a new folder
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        public IActionResult NewFolder(string folderId)
        {
            return View(model: folderId);
        }

        // POST /Files/NewFolder
        // Creates a new folder
        // folderId: The ID of the folder to create the new folder in
        // newFolderName: The name of the new folder
        [AuthorizeForScopes(Scopes = new [] { GraphConstants.FilesReadWrite })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewFolder(string folderId,
                                                   string newFolderName)
        {
            if (string.IsNullOrEmpty(folderId))
            {
                return RedirectToAction("NewFolder")
                    .WithError("No folder ID was specified");
            }

            if (string.IsNullOrEmpty(newFolderName))
            {
                return RedirectToAction("NewFolder")
                    .WithError("No folder name was specified");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // PUT /me/drive/items/folderId:/fileName:/content
                var newFolder = await graphClient.Me
                    .Drive
                    .Items[folderId]
                    .Children
                    .Request()
                    .AddAsync(new DriveItem
                    {
                        Name = newFolderName,
                        Folder = new Folder()
                    });

                return RedirectToAction("Index", new { folderId = newFolder.Id })
                    .WithSuccess("Folder created");
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index", new { folderId = folderId })
                    .WithError("Error creating folder", ex.Error.Message);
            }
        }

        // Builds a FilesViewDisplayModel
        // folderId: ID of the folder to get files and folders from. If null, gets from the root
        // pageRequestUrl: Used for paging requests to get the next set of results
        private async Task<IActionResult> GetViewForFolder(string folderId = null,
                                                           string pageRequestUrl = null)
        {
            var model = new FilesViewDisplayModel();

            try
            {
                var graphClient = GetGraphClientForScopes(_filesScopes);

                // Get selected folder
                IDriveItemRequest folderRequest;
                if (string.IsNullOrEmpty(folderId))
                {
                    // Get the root
                    // GET /me/drive/root
                    folderRequest = graphClient.Me
                        .Drive
                        .Root
                        .Request();
                }
                else
                {
                    // GET /me/drive/items/folderId
                    folderRequest = graphClient.Me
                        .Drive
                        .Items[folderId]
                        .Request();
                }

                // Send the request
                model.SelectedFolder = await folderRequest
                    // Only select the fields used by the app
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.ParentReference
                    })
                    .GetAsync();

                // Get files and folders
                IDriveItemChildrenCollectionRequest itemRequest;

                // Is this a page request?
                if (!string.IsNullOrEmpty(pageRequestUrl))
                {
                    // Instead of using the request builders,
                    // initialize the request directly from the supplied
                    // URL
                    itemRequest = new DriveItemChildrenCollectionRequest(
                        pageRequestUrl, graphClient, null);
                }
                else if (string.IsNullOrEmpty(folderId))
                {
                    // No folder ID specified, so use /me/drive/root/children
                    // to get all items in the root of user's drive
                    // GET /me/drive/root/children
                    itemRequest = graphClient.Me
                        .Drive
                        .Root
                        .Children
                        .Request();
                }
                else
                {
                    // Folder ID specified
                    // GET /me/drive/items/folderId/children
                    itemRequest =  graphClient.Me
                        .Drive
                        .Items[folderId]
                        .Children
                        .Request();
                }

                if (string.IsNullOrEmpty(pageRequestUrl))
                {
                    itemRequest = itemRequest
                        .Top(GraphConstants.PageSize)
                        // Only get the fields used by the view
                        .Select(d => new
                        {
                            d.File,
                            d.FileSystemInfo,
                            d.Folder,
                            d.Id,
                            d.LastModifiedBy,
                            d.Name,
                            d.ParentReference
                        })
                        .Expand("thumbnails");
                }

                // Get max PageSize number of results
                var driveItemPage = await itemRequest
                    .GetAsync();

                model.Files = new List<DriveItem>();
                model.Folders = new List<DriveItem>();

                foreach (var item in driveItemPage.CurrentPage)
                {
                    if (item.Folder != null)
                    {
                        model.Folders.Add(item);
                    }
                    else if (item.File != null)
                    {
                        model.Files.Add(item);
                    }
                }

                model.NextPageUrl = driveItemPage.NextPageRequest?
                    .GetHttpRequestMessage().RequestUri.ToString();

                return View("Index", model);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Error", "Home")
                    .WithError($"Error getting files", ex.Error.Message);
            }
        }
    }
}
