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
    public partial class ItemManager
    {
        private static ItemManager defaultInstance = new ItemManager();
        private IMobileServiceClient client;
        private IMobileServiceSyncTable<Item> itemTable;

        private ItemManager()
        {
            this.client = new MobileServiceClient(Settings.BASE_ADDRESS);
            var store = new MobileServiceSQLiteStore("localitemstore.db");
            store.DefineTable<Item>();
            var first = Task.Run(async () =>
            {
                try
                {
                    await this.client.SyncContext.InitializeAsync(store);
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
                    this.itemTable = this.client.GetSyncTable<Item>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            });
        }

        public static ItemManager DefaultManager

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
            get { return this.itemTable is IMobileServiceSyncTable<Item>; }
        }

        public async Task<ObservableCollection<Item>> GetItemsAsync(bool syncItems = false)

        {
            try

            {
                if (syncItems)

                {
                    await this.SyncAsync();
                }

                IEnumerable<Item> items = await this.itemTable

                                    .Where(todoItem => todoItem.Name != string.Empty)

                                    .ToEnumerableAsync();

                return new ObservableCollection<Item>(items);
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

        public async Task SaveTaskAsync(Item item)

        {
            await this.itemTable.InsertAsync(item);
        }

        public async Task SyncAsync()

        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try

            {
                await this.client.SyncContext.PushAsync();

                await this.itemTable.PullAsync(

                    //The first parameter is a query name that is used internally by the client SDK to implement incremental sync.

                    //Use a different query name for each unique query in your program

                    "allItems",

                    this.itemTable.CreateQuery());
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