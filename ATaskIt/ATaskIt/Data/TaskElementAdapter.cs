using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    internal class ElementAdapter : BaseAdapter<Item>
    {
        private Context myContext;
        private List<Item> myItemList;
        private ObservableCollection<Status> status_enum_async;
        private ItemManager status_manager;

        public ElementAdapter(Context context, List<Item> list, ItemManager status_manager, ObservableCollection<Status> allstatus)
        {
            this.myItemList = list;
            this.myContext = context;
            this.status_manager = status_manager;
            this.status_enum_async = allstatus;
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
            var row = convertView;
            if (row == null)
            {
                row = LayoutInflater.From(this.myContext).Inflate(Resource.Layout.TaskElement, null, false);
            }

            var taskName = row.FindViewById<TextView>(Resource.Id.TaskName);
            taskName.Text = this.myItemList[position].Name;

            var done = row.FindViewById<CheckBox>(Resource.Id.Done);
            try
            {
                var status = this.status_enum_async
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

            var adapter = new ArrayAdapter(this.myContext, Android.Resource.Layout.SimpleSpinnerItem, users);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            assigned.Adapter = adapter;

            try
            {
                var status = this.status_enum_async
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
            assigned.ItemSelected -= this.User_Assigned;
            assigned.ItemSelected += this.User_Assigned;
            return row;
        }

        private async void Done_Checked(object sender, EventArgs e)
        {
            var checked_name = (sender as CheckBox).Tag.ToString();
            var blocker = new Object();
            var status = this.status_enum_async
                                    .Where(u => u.Name == checked_name);

            if (status.Any())
            {
                var _executed = (sender as CheckBox).Checked;
                if (status.FirstOrDefault().Executed != _executed)
                {
                    status.FirstOrDefault().Executed = (sender as CheckBox).Checked;
                    await this.status_manager.SaveStatusAsync(status.FirstOrDefault());
                }
            }
            else
                await this.status_manager.SaveStatusAsync(new Status { Name = checked_name, Executed = (sender as CheckBox).Checked });

            Console.WriteLine(checked_name + " is " + (sender as CheckBox).Checked);
        }

        private async void User_Assigned(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (sender != null)
            {
                var selected_product = (sender as Spinner).Tag.ToString();
                var selected_user = (sender as Spinner).SelectedItem.ToString();
                var status = this.status_enum_async
                                        .Where(u => u.Name == selected_product);

                if (status.Any())
                {
                    if (status.FirstOrDefault().Selected != selected_user)
                    {
                        status.FirstOrDefault().Selected = selected_user;
                        await this.status_manager.SaveStatusAsync(status.FirstOrDefault());
                    }
                }
                else
                    await this.status_manager.SaveStatusAsync(new Status { Name = selected_product, Selected = selected_user });

                Console.WriteLine(selected_user + " assigned to " + selected_product);
            }
        }
    }
}