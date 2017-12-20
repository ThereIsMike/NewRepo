using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    class TaskElementAdapter : BaseAdapter<Item>
    {
        private List<Item> myItemList;
        private Context myContext;
        IMobileServiceTable<Item> myItemTable;

        public TaskElementAdapter(Context context, List<Item> list, IMobileServiceTable<Item> table)
        {
            this.myItemList = list;
            this.myContext = context;
            this.myItemTable = table;

        }
        public override Item this[int position]
        {
            get { return this.myItemList[position]; }
        }

        public override int Count
        {
            get { return this.myItemList.Count(); }
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            if(row == null)
            {
                row = LayoutInflater.From(this.myContext).Inflate(Resource.Layout.TaskElement, null, false);
            }



            TextView taskName = row.FindViewById<TextView>(Resource.Id.TaskName);
            taskName.Text = this.myItemList[position].Name;
            taskName.LongClick += this.TaskName_LongClick;

            Switch done = row.FindViewById<Switch>(Resource.Id.Done);
            done.Checked = false;

            Spinner assigned = row.FindViewById<Spinner>(Resource.Id.Assigned);
            List<string> users = new List<string> { "Anna", "Michal" }; 
            var adapter = new ArrayAdapter(assigned.Context, Android.Resource.Layout.SimpleSpinnerItem, users);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            assigned.Adapter = adapter;

            return row;
        }

        private async void TaskName_LongClick(object sender, View.LongClickEventArgs e)
        {
            var ItemToDelete = (sender as TextView).Text;
            Console.WriteLine($"Delete {ItemToDelete}");
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