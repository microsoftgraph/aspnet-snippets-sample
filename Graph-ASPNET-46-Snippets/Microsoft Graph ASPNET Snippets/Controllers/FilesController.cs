/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using System.Text;
using System.IO;
using Resources;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{

    [Authorize]
    public class FilesController : Controller
    {
        public ActionResult Index()
        {
            return View("Files");
        }

        // Get the drive items in the root directory of the current user's default drive.
        public async Task<ActionResult> GetMyFilesAndFolders()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                
                // Get the files and folders in the current user's drive.
                IDriveItemChildrenCollectionPage driveItems = await graphClient.Me.Drive.Root.Children.Request().GetAsync();

                if (driveItems?.Count > 0)
                {
                    foreach (DriveItem fileOrFolder in driveItems)
                    {

                        // Get file and folder properties.
                        string type = fileOrFolder.File?.ToString() ?? fileOrFolder.Folder?.ToString();
                        items.Add(new ResultsItem
                        {
                            Display = fileOrFolder.Name + " (" + type.Replace("Microsoft.Graph.", "") + ")",
                            Id = fileOrFolder.Id
                        });
                    }
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the shared items.
                IDriveSharedWithMeCollectionPage driveItems = await graphClient.Me.Drive.SharedWithMe().Request().GetAsync();

                if (driveItems?.Count > 0)
                {
                    foreach (DriveItem driveItem in driveItems)
                    {
                        ResultsItem item = new ResultsItem();

                        // Get item properties.
                        string type = driveItem.RemoteItem.File?.ToString() ?? driveItem.RemoteItem.Folder?.ToString();
                        item.Properties.Add(Resource.Prop_Name, driveItem.RemoteItem.Name + " (" + type.Replace("Microsoft.Graph.", "") + ")");
                        item.Properties.Add(Resource.Prop_Id, driveItem.RemoteItem.Id);

                        items.Add(item);
                    }
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the current user's default drive.
                Drive drive = await graphClient.Me.Drive.Request().GetAsync();
                
                if (drive != null)
                {

                    // Get drive properties.
                    item.Properties.Add(Resource.Prop_Type, drive.DriveType);
                    item.Properties.Add(Resource.Prop_Quota, drive.Quota.State);
                    item.Properties.Add(Resource.Prop_Id, drive.Id);

                    items.Add(item);
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            string guid = Guid.NewGuid().ToString();
                                
            // Create the file to upload. Read the file content string into a stream that gets passed as the file content.
            string fileName = Resource.File + guid.Substring(0, 8) + ".txt";
            byte[] byteArray = Encoding.ASCII.GetBytes(Resource.FileContent_New);

            using (MemoryStream fileContentStream = new MemoryStream(byteArray))
            {
                try
                {

                    // Initialize the GraphServiceClient.
                    GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                    // Add the file.
                    DriveItem file = await graphClient.Me.Drive.Root.ItemWithPath(fileName).Content.Request().PutAsync<DriveItem>(fileContentStream);

                    if (file != null)
                    {

                        // Get file properties.
                        item.Display = file.Name;
                        item.Id = file.Id;
                        item.Properties.Add(Resource.Prop_Created, file.CreatedDateTime.Value.ToLocalTime());
                        item.Properties.Add(Resource.Prop_Url, file.WebUrl);
                        item.Properties.Add(Resource.Prop_Id, file.Id);

                        items.Add(item);
                    }
                    results.Items = items;
                }
                catch (ServiceException se)
                {
                    if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                    return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
                }
            }
            return View("Files", results);
        }

        // Create a folder in the current user's root directory. 
        public async Task<ActionResult> CreateFolder()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            string guid = Guid.NewGuid().ToString();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Add the folder.
                DriveItem folder = await graphClient.Me.Drive.Root.Children.Request().AddAsync(new DriveItem
                {
                    Name = Resource.Folder + guid.Substring(0, 8),
                    Folder = new Folder()
                });

                if (folder != null)
                {

                    // Get folder properties.
                    item.Display = folder.Name;
                    item.Id = folder.Id;
                    item.Properties.Add(Resource.Prop_Created, folder.CreatedDateTime.Value.ToLocalTime());
                    item.Properties.Add(Resource.Prop_ChildCount, folder.Folder.ChildCount);
                    item.Properties.Add(Resource.Prop_Id, folder.Id);

                    items.Add(item);
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the file or folder object.
                DriveItem fileOrFolder = await graphClient.Me.Drive.Items[id].Request().GetAsync();

                if (fileOrFolder != null)
                {

                    // Get file or folder properties.
                    item.Display = fileOrFolder.Name;
                    item.Id = fileOrFolder.Id;
                    item.Properties.Add(Resource.Prop_Type, fileOrFolder.File?.ToString() ?? fileOrFolder.Folder?.ToString());
                    item.Properties.Add(Resource.Prop_Url, fileOrFolder.WebUrl);
                    item.Properties.Add(Resource.Prop_Id, fileOrFolder.Id);

                    items.Add(item);
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the item and make sure it's a file.
                DriveItem file = await graphClient.Me.Drive.Items[id].Request().GetAsync();
                
                if (file.File != null)
                {

                    // Get the file content.
                    Stream stream = await graphClient.Me.Drive.Items[id].Content.Request().GetAsync();

                    // Get file properties.
                    item.Display = file.Name;
                    item.Id = file.Id;
                    item.Properties.Add(Resource.Prop_Url, file.WebUrl);
                    item.Properties.Add(Resource.Prop_DownloadUrl, file.AdditionalData["@microsoft.graph.downloadUrl"]);
                    item.Properties.Add(Resource.Prop_StreamLength, stream.Length);
                    item.Properties.Add(Resource.Prop_Id, file.Id);
                }
                else
                {

                    // Selected item is not a file.
                    results.Selectable = false;
                    item.Properties.Add(Resource.File_ChooseFile, "");
                }
                items.Add(item);
                results.Items = items;
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
        public async Task<ActionResult> UpdateFileOrFolderMetadata(string id, string name)
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            name = name.Replace(" (" + Resource.File + ")", "");
            name = name.Replace(" (" + Resource.Folder + ")", "");

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Update the item.
                DriveItem fileOrFolder = await graphClient.Me.Drive.Items[id].Request().UpdateAsync(new DriveItem
                {
                    Name = Resource.Updated + name
                });

                if (fileOrFolder != null)
                {
                    
                    // Get file or folder properties.
                    item.Display = fileOrFolder.Name;
                    item.Id = fileOrFolder.Id;
                    item.Properties.Add(Resource.Prop_Updated, fileOrFolder.LastModifiedDateTime.Value.ToLocalTime());
                    item.Properties.Add(Resource.Prop_Url, fileOrFolder.WebUrl);
                    item.Properties.Add(Resource.Prop_Id, fileOrFolder.Id);

                    items.Add(item);
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the file. Make sure it's a .txt file (for the purposes of this snippet).
                DriveItem selectedFile = await graphClient.Me.Drive.Items[id].Request().GetAsync();
                
                if (selectedFile.File != null && selectedFile.Name.EndsWith(".txt"))
                {

                    // Read the file content string into a stream that gets passed as the file content.
                    byte[] byteArray = Encoding.ASCII.GetBytes(Resource.FileContent_Updated);
                    using (MemoryStream fileContentStream = new MemoryStream(byteArray))
                    {

                        // Update the file.
                        DriveItem file = await graphClient.Me.Drive.Items[id].Content.Request().PutAsync<DriveItem>(fileContentStream);
                        
                        if (file.File != null)
                        {

                            // Get file properties.
                            item.Display = file.Name;
                            item.Id = file.Id;
                            item.Properties.Add(Resource.Prop_Updated, file.LastModifiedDateTime.Value.ToLocalTime());
                            item.Properties.Add(Resource.Prop_Url, file.WebUrl);
                            item.Properties.Add(Resource.Prop_Id, file.Id);
                        }
                    }
                }
                else
                {

                    // Selected item is not supported.
                    results.Selectable = false;
                    item.Properties.Add(Resource.File_ChooseTextFile, "");
                }
                items.Add(item);
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            results.Selectable = false;

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                    
                // Delete the item.
                await graphClient.Me.Drive.Items[id].Request().DeleteAsync();
                
                // This operation doesn't return anything.
                item.Properties.Add(Resource.No_Return_Data, "");
                items.Add(item);
                results.Items = items;
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
