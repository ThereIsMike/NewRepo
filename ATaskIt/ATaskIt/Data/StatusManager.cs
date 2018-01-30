using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace ATaskIt.Data
{
    public partial class StatusManager
    {
        private static StatusManager defaultInstance = new StatusManager();
        private IMobileServiceClient client;
        private IMobileServiceSyncTable<Status> statusTable;

        private StatusManager()
        {
            this.client = new MobileServiceClient(Settings.BASE_ADDRESS);

            this.Store = new MobileServiceSQLiteStore("localitemstore.db");
            this.Store.DefineTable<Status>();
            var first = Task.Run(async () =>
            {
                try
                {
                    await this.client.SyncContext.InitializeAsync(this.Store);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            });
            first.ContinueWith(x =>
            {
                try
                {
                    this.statusTable = this.client.GetSyncTable<Status>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            });
        }

        public static StatusManager DefaultManager

        {
            get

            {
                return defaultInstance;
            }

            private set

            {
                defaultInstance = value;
            }
        }

        public IMobileServiceClient CurrentClient

        {
            get { return this.client; }
        }

        public bool IsOfflineEnabled

        {
            get { return this.statusTable is IMobileServiceSyncTable<Status>; }
        }

        public MobileServiceSQLiteStore Store { get; set; }

        public async Task DeleteStatusAsync(Status status)

        {
            await this.statusTable.DeleteAsync(status);
        }

        public async Task<ObservableCollection<Status>> GetStatusAsync(bool syncStatus = false)

        {
            try

            {
                if (syncStatus)

                {
                    await this.SyncAsync();
                }

                IEnumerable<Status> status = await this.statusTable

                                    .Where(todoItem => todoItem.Name != string.Empty)

                                    .ToEnumerableAsync();

                return new ObservableCollection<Status>(status);
            }
            catch (MobileServiceInvalidOperationException msioe)

            {
                System.Diagnostics.Debug.WriteLine($"Invalid sync operation: {0}", msioe.Message);
            }
            catch (Exception e)

            {
                System.Diagnostics.Debug.WriteLine($"Sync error: {0}", e.Message);
            }

            return null;
        }

        public async Task SaveStatusAsync(Status status)

        {
            await this.statusTable.InsertAsync(status);
        }

        public async Task SyncAsync()

        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try

            {
                await this.client.SyncContext.PushAsync();

                await this.statusTable.PullAsync(

                    //The first parameter is a query name that is used internally by the client SDK to implement incremental sync.

                    //Use a different query name for each unique query in your program

                    "allStatus",

                    this.statusTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)

            {
                if (exc.PushResult != null)

                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors
            // like network conditions,

            // server conflicts and others via the IMobileServiceSyncHandler.

            if (syncErrors != null)

            {
                foreach (var error in syncErrors)

                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)

                    {
                        //Update failed, reverting to server's copy.

                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else

                    {
                        // Discard local change.

                        await error.CancelAndDiscardItemAsync();
                    }

                    System.Diagnostics.Debug.WriteLine($"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }
        }
    }
}