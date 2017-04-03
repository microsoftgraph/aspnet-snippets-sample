/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{
    public class FilesService
    {

        // Get the drive items in the root directory of the current user's default drive.
        public async Task<List<ResultsItem>> GetMyFilesAndFolders(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

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
                        Display = fileOrFolder.Name + " (" + type?.Replace("Microsoft.Graph.", "") + ")",
                        Id = fileOrFolder.Id
                    });
                }
            }
            return items;
        }

        // Get the items that are shared with the current user.
        public async Task<List<ResultsItem>> GetSharedWithMe(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get shared items.
            IDriveSharedWithMeCollectionPage driveItems = await graphClient.Me.Drive.SharedWithMe().Request().GetAsync();

            if (driveItems?.Count > 0)
            {
                foreach (DriveItem driveItem in driveItems)
                {

                    // Get item properties.
                    string type = driveItem.RemoteItem.File?.ToString() ?? driveItem.RemoteItem.Folder?.ToString();
                    items.Add(new ResultsItem
                    {
                        Properties = new Dictionary<string, object>
                        {
                            { Resource.Prop_Name, driveItem.RemoteItem.Name + " (" + type?.Replace("Microsoft.Graph.", "") + ")" },
                            { Resource.Prop_Id, driveItem.RemoteItem.Id }
                        }
                    });
                }
            }
            return items;
        }

        // Get the current user's default drive.
        public async Task<List<ResultsItem>> GetMyDrive(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Get the current user's default drive.
            Drive drive = await graphClient.Me.Drive.Request().GetAsync();

            if (drive != null)
            {

                // Get drive properties.
                items.Add(new ResultsItem
                {
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Type, drive.DriveType },
                        { Resource.Prop_Quota, drive.Quota.State },
                        { Resource.Prop_Id, drive.Id}
                    }
                });
            }
            return items;
        }

        // Create a text file in the current user's root directory.
        public async Task<List<ResultsItem>> CreateFile(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Create the file to upload. Read the file content string into a stream that gets passed as the file content.
            string guid = Guid.NewGuid().ToString();
            string fileName = Resource.File + guid.Substring(0, 8) + ".txt";
            byte[] byteArray = Encoding.ASCII.GetBytes(Resource.FileContent_New);

            using (MemoryStream fileContentStream = new MemoryStream(byteArray))
            {

                // Add the file.
                DriveItem file = await graphClient.Me.Drive.Root.ItemWithPath(fileName).Content.Request().PutAsync<DriveItem>(fileContentStream);

                if (file != null)
                {

                    // Get file properties.
                    items.Add(new ResultsItem
                    {
                        Display = file.Name,
                        Id = file.Id,
                        Properties = new Dictionary<string, object>
                        {
                            { Resource.Prop_Created, file.CreatedDateTime.Value.ToLocalTime() },
                            { Resource.Prop_Url, file.WebUrl },
                            { Resource.Prop_Id, file.Id }
                        }
                    });
                }
            }
            return items;
        }

        // Create a folder in the current user's root directory. 
        public async Task<List<ResultsItem>> CreateFolder(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            string guid = Guid.NewGuid().ToString();

            // Add the folder.
            DriveItem folder = await graphClient.Me.Drive.Root.Children.Request().AddAsync(new DriveItem
            {
                Name = Resource.Folder + guid.Substring(0, 8),
                Folder = new Folder()
            });

            if (folder != null)
            {

                // Get folder properties.
                items.Add(new ResultsItem
                {
                    Display = folder.Name,
                    Id = folder.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Created, folder.CreatedDateTime.Value.ToLocalTime() },
                        { Resource.Prop_ChildCount, folder.Folder.ChildCount },
                        { Resource.Prop_Id, folder.Id }
                    }
                });
            }
            return items;
        }


        // Uploads a large file to the current user's root directory.
        public async Task<List<ResultsItem>> UploadLargeFile(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            using (Stream fileStream = System.IO.File.OpenRead(HostingEnvironment.MapPath("/Content/LargeFileUploadResource.bmp")))
            {
                // Create the upload session. The access token is no longer required as you have session established for the upload.  
                // POST /v1.0/drive/root:/UploadLargeFile.bmp:/microsoft.graph.createUploadSession
                UploadSession uploadSession = await graphClient.Me.Drive.Root.ItemWithPath("LargeFileUploadResource.bmp").CreateUploadSession().Request().PostAsync();

                int maxChunkSize = 320 * 1024; // 320 KB - Change this to your chunk size. 5MB is the default.
                ChunkedUploadProvider provider = new ChunkedUploadProvider(uploadSession, graphClient, fileStream, maxChunkSize);

                // Set up the chunk request necessities.
                IEnumerable<UploadChunkRequest> chunkRequests = provider.GetUploadChunkRequests();
                byte[] readBuffer = new byte[maxChunkSize];
                List<Exception> trackedExceptions = new List<Exception>();
                DriveItem uploadedFile = null;

                // Upload the chunks.
                foreach (var request in chunkRequests)
                {
                    // Do your updates here: update progress bar, etc.
                    // ...
                    // Send chunk request
                    UploadChunkResult result = await provider.GetChunkRequestResponseAsync(request, readBuffer, trackedExceptions);

                    if (result.UploadSucceeded)
                    {
                        uploadedFile = result.ItemResponse;
                        
                        // Get file properties.
                        items.Add(new ResultsItem
                        {
                            Display = uploadedFile.Name,
                            Id = uploadedFile.Id,
                            Properties = new Dictionary<string, object>
                            {
                                { Resource.Prop_Created, uploadedFile.CreatedDateTime.Value.ToLocalTime() },
                                { Resource.Prop_Url, uploadedFile.WebUrl },
                                { Resource.Prop_Id, uploadedFile.Id }
                            }
                        });
                    }
                }

                // Check that upload succeeded.
                if (uploadedFile == null)
                {
                    // Retry the upload
                    // ...
                }
            }
            return items;
        }

        // Get a file or folder (metadata) in the current user's drive.
        public async Task<List<ResultsItem>> GetFileOrFolderMetadata(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Get the file or folder object.
            DriveItem fileOrFolder = await graphClient.Me.Drive.Items[id].Request().GetAsync();

            if (fileOrFolder != null)
            {

                // Get file or folder properties.
                items.Add(new ResultsItem
                {
                    Display = fileOrFolder.Name,
                    Id = fileOrFolder.Id,
                    Properties = new Dictionary<string, object>
                        {
                            { Resource.Prop_Type, fileOrFolder.File?.ToString() ?? fileOrFolder.Folder?.ToString() },
                            { Resource.Prop_Url, fileOrFolder.WebUrl },
                            { Resource.Prop_Id, fileOrFolder.Id }
                        }
                });
            }
            return items;
        }

        // Download the content of an existing file.
        // This snippet returns the length of the file stream and some file metadata properties.
        public async Task<List<ResultsItem>> DownloadFile(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Get the item and make sure it's a file.
            DriveItem file = await graphClient.Me.Drive.Items[id].Request().GetAsync();

            if (file.File != null)
            {

                // Get the file content.
                using (Stream stream = await graphClient.Me.Drive.Items[id].Content.Request().GetAsync())
                {
                    
                    // Get file properties.
                    items.Add(new ResultsItem
                    {
                        Display = file.Name,
                        Id = file.Id,
                        Properties = new Dictionary<string, object>
                        {
                            { Resource.Prop_Url, file.WebUrl },
                            { Resource.Prop_DownloadUrl, file.AdditionalData["@microsoft.graph.downloadUrl"] }, // Temporary workaround for a known SDK issue.
                            { Resource.Prop_StreamLength, stream.Length},
                            { Resource.Prop_Id, file.Id }
                        }
                    });
                }
            }
            else
            {

                // Selected item is not a file.
                items.Add(new ResultsItem
                {
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.File_ChooseFile, "" }
                    }
                });
            }
            return items;
        }

        // Update the metadata of a file or folder. 
        // This snippet updates the item's name. 
        // To move an item, point the ParentReference.Id or ParentReference.Path property to the target destination.
        public async Task<List<ResultsItem>> UpdateFileOrFolderMetadata(GraphServiceClient graphClient, string id, string name)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            name = name.Replace(" (" + Resource.File + ")", "");
            name = name.Replace(" (" + Resource.Folder + ")", "");

            // Update the item.
            DriveItem fileOrFolder = await graphClient.Me.Drive.Items[id].Request().UpdateAsync(new DriveItem
            {
                Name = Resource.Updated + name.TrimEnd()

                // The following example moves an item by updating the item's ParentReference.Id property.
                //ParentReference = new ItemReference 
                //{
                //    Id = {destination-folder-id}
                //}
            });

            if (fileOrFolder != null)
            {

                // Get file or folder properties.
                items.Add(new ResultsItem
                {
                    Display = fileOrFolder.Name,
                    Id = fileOrFolder.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Updated, fileOrFolder.LastModifiedDateTime.Value.ToLocalTime() },
                        { Resource.Prop_Url, fileOrFolder.WebUrl },
                        { Resource.Prop_Id, fileOrFolder.Id }
                    }
                });
            }
            return items;
        }

        // Update the content of a file in the user's root directory. 
        // This snippet replaces the text content of a .txt file.
        public async Task<List<ResultsItem>> UpdateFileContent(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

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
                        items.Add(new ResultsItem
                        {
                            Display = file.Name,
                            Id = file.Id,
                            Properties = new Dictionary<string, object>
                            {
                                { Resource.Prop_Updated, file.LastModifiedDateTime.Value.ToLocalTime() },
                                { Resource.Prop_Url, file.WebUrl },
                                { Resource.Prop_Id, file.Id }
                            }
                        });
                    }
                }
            }
            else
            {

                // Selected item is not supported.
                items.Add(new ResultsItem
                {
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.File_ChooseTextFile, "" }
                    }
                });
            }
            return items;
        }

        // Delete a file in the user's root directory.
        public async Task<List<ResultsItem>> DeleteFileOrFolder(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Delete the item.
            await graphClient.Me.Drive.Items[id].Request().DeleteAsync();

            // This operation doesn't return anything.
            items.Add(new ResultsItem
            {
                Properties = new Dictionary<string, object>
                {
                    { Resource.No_Return_Data, "" }
                }
            });
            return items;
        }

        // Get a sharing link.
        // This snippet gets a link that has `view` permissions to the file.
        public async Task<List<ResultsItem>> GetSharingLink(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get a sharing link for the file.
            Permission permission = await graphClient.Me.Drive.Items[id].CreateLink("view").Request().PostAsync();

            if (permission != null)
            {

                // Get permission properties.
                items.Add(new ResultsItem
                {
                    Display = permission.Link.WebUrl,
                    Id = permission.Id
                });
            }
            return items;
        }
    }
}
 