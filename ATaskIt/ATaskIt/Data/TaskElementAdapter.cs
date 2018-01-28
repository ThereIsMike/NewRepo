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
        //private Switch done;
        private Context myContext;

        private List<Item> myItemList;
        private MobileServiceCollection<Data.Status, Data.Status> myItemStatus;
        private IMobileServiceTable<Status> myItemStatusTable;

        //private IMobileServiceTable<Item> myItemTable;

        public TaskElementAdapter(Context context, List<Item> list,

            //IMobileServiceTable<Item> table,
            IMobileServiceTable<Status> statustable,
            MobileServiceCollection<Data.Status, Data.Status> status)
        {
            this.myItemList = list;
            this.myContext = context;

            //this.myItemTable = table;
            this.myItemStatusTable = statustable;
            this.myItemStatus = status;
        }

        public override int Count
        {
            get { return this.myItemList.Count; }
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

            var taskName = row.FindViewById<TextView>(Resource.Id.TaskName);
            taskName.Text = this.myItemList[position].Name;

            var done = row.FindViewById<CheckBox>(Resource.Id.Done);
            try
            {
                var status = this.myItemStatus
                                .Where(u => u.Name == taskName.Text);
                if (status.Any())
                    done.Checked = status.FirstOrDefault().Executed;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            done.Tag = this.myItemList[position].Name;
            done.CheckedChange -= this.Done_Checked;
            done.CheckedChange += this.Done_Checked;

            var assigned = row.FindViewById<Spinner>(Resource.Id.Assigned);
            assigned.Tag = this.myItemList[position].Name;

            var users = new List<string> { "None", "Anna", "Michal" };
            assigned.ItemSelected -= this.User_Assigned;
            assigned.ItemSelected += this.User_Assigned;
            var adapter = new ArrayAdapter(this.myContext, Android.Resource.Layout.SimpleSpinnerItem, users);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            assigned.Adapter = adapter;

            try
            {
                var status = this.myItemStatus
                                .Where(u => u.Name == taskName.Text);
                if (status.Any())
                {
                    var sss = users.Select((x, n) =>
                    {
                        if (x == status.FirstOrDefault().Selected)
                            return n;
                        return 0;
                    }).Where(g => g > 0).FirstOrDefault();
                    assigned.SetSelection(sss, false);
                    Console.WriteLine(assigned.Tag + " belongs to  " + sss);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return row;
        }

        private async void Done_Checked(object sender, EventArgs e)
        {
            var checked_name = (sender as CheckBox).Tag.ToString();
            var blocker = new Object();
            var status = await this.myItemStatusTable
                                    .Where(u => u.Name == checked_name)
                                    .ToCollectionAsync().ConfigureAwait(false);
            if (status.Count > 0)
            {
                await this.myItemStatusTable.DeleteAsync(status.FirstOrDefault());
                status.FirstOrDefault().Executed = (sender as CheckBox).Checked;
                await this.myItemStatusTable.InsertAsync(status.FirstOrDefault());
            }
            else
                await this.myItemStatusTable.InsertAsync(new Status { Name = checked_name, Executed = (sender as CheckBox).Checked });

            Console.WriteLine(checked_name + " is " + (sender as CheckBox).Checked);
        }

        private async void User_Assigned(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (sender != null)
            {
                var selected_product = (sender as Spinner).Tag.ToString();
                var selected_user = (sender as Spinner).SelectedItem.ToString();
                var status = await this.myItemStatusTable
                                        .Where(u => u.Name == selected_product)
                                        .ToCollectionAsync().ConfigureAwait(false);
                if (status.Count > 0)
                {
                    await this.myItemStatusTable.DeleteAsync(status.FirstOrDefault());
                    status.FirstOrDefault().Selected = selected_user;
                    await this.myItemStatusTable.InsertAsync(status.FirstOrDefault());
                }
                else
                    await this.myItemStatusTable.InsertAsync(new Status { Name = selected_product, Selected = selected_user });

                Console.WriteLine(selected_user + " assigned to " + selected_product);
            }
        }
    }
}