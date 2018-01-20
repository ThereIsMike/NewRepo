using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;

namespace ATaskIt.Data
{
    internal class TaskElementAdapter : BaseAdapter<Item>
    {
        private Context myContext;
        private List<Item> myItemList;
        private MobileServiceCollection<Data.Status, Data.Status> myItemStatus;
        private IMobileServiceTable<Status> myItemStatusTable;
        private IMobileServiceTable<Item> myItemTable;

        public TaskElementAdapter(Context context, List<Item> list,
            IMobileServiceTable<Item> table,
            IMobileServiceTable<Status> statustable,
            MobileServiceCollection<Data.Status, Data.Status> status)
        {
            this.myItemList = list;
            this.myContext = context;
            this.myItemTable = table;
            this.myItemStatusTable = statustable;
            this.myItemStatus = status;
        }

        public override int Count
        {
            get { return this.myItemList.Count(); }
        }

        public override Item this[int position]
        {
            get { return this.myItemList[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            if (row == null)
            {
                row = LayoutInflater.From(this.myContext).Inflate(Resource.Layout.TaskElement, null, false);
            }

            TextView taskName = row.FindViewById<TextView>(Resource.Id.TaskName);
            taskName.Text = this.myItemList[position].Name;
            taskName.LongClick += this.TaskName_LongClick;

            Switch done = row.FindViewById<Switch>(Resource.Id.Done);
            var status = this.myItemStatus
                            .Where(u => u.Name == taskName.Text);
            if (status.Count() > 0)
                done.Checked = status.FirstOrDefault().Executed;
            done.Tag = this.myItemList[position].Name;

            done.CheckedChange += this.Done_Checked;

            Spinner assigned = row.FindViewById<Spinner>(Resource.Id.Assigned);
            List<string> users = new List<string> { "Anna", "Michal" };
            var adapter = new ArrayAdapter(assigned.Context, Android.Resource.Layout.SimpleSpinnerItem, users);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            assigned.Adapter = adapter;

            return row;
        }

        private async void Done_Checked(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            var checked_name = (sender as Switch).Tag.ToString();
            Console.WriteLine("Checked: " + checked_name);
            var status = await this.myItemStatusTable
                                  .Where(u => u.Name == checked_name)
                                  .ToCollectionAsync().ConfigureAwait(false);

            if (status.Count > 0)
            {
                await this.myItemStatusTable.DeleteAsync(status.FirstOrDefault());
                status.FirstOrDefault().Executed = e.IsChecked;
                await this.myItemStatusTable.InsertAsync(status.FirstOrDefault());
            }
            else
                await this.myItemStatusTable.InsertAsync(new Status { Name = checked_name, Executed = e.IsChecked });
            Console.WriteLine(checked_name + "is " + e.IsChecked);
        }

        private async void TaskName_LongClick(object sender, View.LongClickEventArgs e)
        {
            var ItemToDelete = (sender as TextView).Text;
            var items = await this.myItemTable
                                .Where(u => u.Name == ItemToDelete)
                                .ToCollectionAsync();
            if (items.Count > 0)
            {
                var find = await this.myItemTable.LookupAsync(items.FirstOrDefault().Id);
                await this.myItemTable.DeleteAsync(find);
                (sender as TextView).SetBackgroundColor(Color.LightCoral);
            }
        }
    }
}