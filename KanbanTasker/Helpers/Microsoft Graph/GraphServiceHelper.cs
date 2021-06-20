﻿using KanbanTasker.Extensions;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace KanbanTasker.Helpers.Microsoft_Graph
{
    /// <summary>
    /// A helper class to interact with the Microsoft Graph SDK.
    /// </summary>
    public static class GraphServiceHelper
    {
        private static GraphServiceClient GraphClient { get; set; }

        /// <summary>
        /// Initializes the graph service client used to make calls to the Microsoft Graph API.
        /// </summary>
        /// <param name="authProvider"></param>
        public static void InitializeClient(IAuthenticationProvider authProvider)
        {
            GraphClient = new GraphServiceClient(authProvider);
        }

        /// <summary>
        /// Gets the graph client used to make calls to Microsoft Graph.
        /// </summary>
        /// <returns>A GraphServiceClient object.</returns>
        public static GraphServiceClient GetGraphClient()
        {
            return GraphClient;
        }

        #region UserRequests

        /// <summary>
        /// Get the current user.
        /// </summary>
        /// <returns>A user object representing the current user.</returns>
        public static async Task<User> GetMeAsync()
        {
            try
            {
                // GET /me
                return await GraphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the current user's email address from their profile.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetMyEmailAddressAsync()
        {
            // Get the current user. 
            // The app only needs the user's email address, so select the mail and userPrincipalName properties.
            // If the mail property isn't defined, userPrincipalName should map to the email for all account types. 
            User me = await GraphClient.Me.Request().Select("mail,userPrincipalName").GetAsync();
            return me.Mail ?? me.UserPrincipalName;
        }

        /// <summary>
        /// Get the current user's display name from their profile.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetMyDisplayNameAsync()
        {
            // Get the current user. 
            // The app only needs the user's displayName
            User me;
            try
            {
                me = await GraphClient.Me.Request().Select("displayName").GetAsync();
                return me.GivenName ?? me.DisplayName;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // MS Graph Known Error 
                    // Users need to sign into personal site / OneDrive at least once
                    // https://docs.microsoft.com/en-us/graph/known-issues#files-onedrive
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Get events from the current user's calendar.
        /// </summary>
        /// <returns>Collection of events from a user's calendar.</returns>
        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            try
            {
                // GET /me/events
                var resultPage = await GraphClient.Me.Events.Request()
                    // Only return the fields used by the application
                    .Select(e => new {
                        e.Subject,
                        e.Organizer,
                        e.Start,
                        e.End
                    })
                    // Sort results by when they were created, newest first
                    .OrderBy("createdDateTime DESC")
                    .GetAsync();

                return resultPage.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting events: {ex.Message}");
                return null;
            }
        }
        // </GetEventsSnippet>

        #endregion UserRequests

        #region OneDriveRequests

        /// <summary>
        /// Get current user's OneDrive root folder.
        /// </summary>
        /// <returns>A DriveItem representing the root folder.</returns>
        public static async Task<DriveItem> GetOneDriveRootAsync()
        {
            try
            {
                // GET /me/drive/root
                return await GraphClient.Me.Drive.Root.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting signed-in users one drive root: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the children of the current user's OneDrive root folder.
        /// </summary>
        /// <returns>A collection of DriveItems.</returns>
        public static async Task<IDriveItemChildrenCollectionPage> GetOneDriveRootChildrenAsync()
        {
            try
            {
                // GET /me/drive/root/children 
                return await GraphClient.Me.Drive.Root.Children.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting signed-in users one drive root children: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the specified folder as a DriveItem. 
        /// </summary>
        /// <param name="folderPath">Path to the data file starting from your local application folder.</param>
        /// <returns>A DriveItem representing the specified folder. Returns null if folder doesn't exist.</returns>
        public static async Task<DriveItem> GetOneDriveFolderAsync(string folderPath)
        {
            try
            {
                // GET /me/drive/root/{folderPath} 
                var searchCollection = await GraphClient.Me.Drive.Root.Search("Kanban Tasker").Request().GetAsync();
                foreach (var folder in searchCollection)
                    if (folder.Name == "Kanban Tasker")
                        return folder;
                return null;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.BadGateway)
                {
                    Console.WriteLine($"Service Exception, Bad Gateway. Error getting signed-in users one drive folder: {ex.Message}");
                }
                else if (ex.IsMatch(GraphErrorCode.GeneralException.ToString()))
                {
                    Console.WriteLine($"General Exception, error getting folder. Please check internet connection.");
                }
                throw;
            }
        }

        /// <summary>
        /// Creates a new folder in the current user's OneDrive root folder.
        /// </summary>
        /// <param name="folderName">Name of the folder to create.</param>
        /// <returns>A DriveItem representing the newly created Folder.</returns>
        public static async Task<DriveItem> CreateNewOneDriveFolderAsync(string folderName)
        {
            try
            {
                var driveItem = new DriveItem
                {
                    
                    Name = folderName,
                    Folder = new Folder
                    {
                    },
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"@microsoft.graph.conflictBehavior","fail"}
                    }
                };

                return await GraphClient.Me.Drive.Root.ItemWithPath("Applications").Children
                    .Request()
                    .AddAsync(driveItem);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, error creating folder in signed-in users one drive root: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Uploads a file in the current user's OneDrive root folder from this applications local folder
        /// using its specified itemId and filename.
        /// </summary>
        /// <param name="itemId">Unique item identifier within a DriveItem (folder/file).</param>
        /// <param name="filename">Name of the datafile.</param>
        /// <returns>A DriveItem representing the newly uploaded file.</returns>
        public static async Task<DriveItem> UploadFileToOneDriveAsync(string itemId, string filename)
        {
            try
            {
                Windows.Storage.StorageFolder storageFolder = 
                    Windows.Storage.ApplicationData.Current.LocalFolder;

                Windows.Storage.StorageFile sampleFile =
                    await storageFolder.GetFileAsync(filename);

                var stream = await sampleFile.OpenStreamForReadAsync();

                return await GraphClient.Me.Drive.Items[itemId].ItemWithPath(filename).Content
                    .Request()
                    .PutAsync<DriveItem>(stream);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Expception, Error uploading file to signed-in users one drive: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Restores the applications data using a backup from the current user's OneDrive. 
        /// <para>Note: Application requires restart after restoring data</para>
        /// </summary>
        /// <param name="itemId">Unique item identifier within a DriveItem (i.e., a folder/file facet).</param>
        /// <param name="filename">Name of the datafile.</param>
        /// <returns></returns>
        public static async Task RestoreFileFromOneDriveAsync(string itemId, string dataFilename)
        {
            try
            {
                // Local storage folder
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;

                // Our local ktdatabase.db file
                Windows.Storage.StorageFile originalDataFile =
                    await storageFolder.GetFileAsync(dataFilename);

                // Stream for the backed up data file
                var backedUpFileStream = await GraphClient.Me.Drive.Items[itemId]
                    .ItemWithPath(dataFilename)
                    .Content
                    .Request()
                    .GetAsync();
                
                // Backed up file
                var backedUpFile = await storageFolder.CreateFileAsync("temp", CreationCollisionOption.ReplaceExisting);
                var newStream = await backedUpFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                // Write data to new file
                using (var outputStream =  newStream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        var buffer = backedUpFileStream.ToByteArray();
                        dataWriter.WriteBytes(buffer);

                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                }

                // Copy and replace local file
                await backedUpFile.CopyAsync(storageFolder, dataFilename, NameCollisionOption.ReplaceExisting);
            }

            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                    Console.WriteLine($"Access Denied: {ex.Message}");

                Console.WriteLine($"Service Exception, Error uploading file to signed-in users one drive: {ex.Message}");
               // return null;
            }
        }

        #endregion OneDriveRequests

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        private static async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                
                // Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}