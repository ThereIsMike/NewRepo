using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ATaskIt.Data
{
    class TaskElementAdapter : BaseAdapter<Item>
    {
        private List<Item> myItemList;
        private Context myContext;

        public TaskElementAdapter(Context context, List<Item> list)
        {
            this.myItemList = list;
            this.myContext = context;

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

            Switch done = row.FindViewById<Switch>(Resource.Id.Done);
            done.Checked = false;

            Spinner assigned = row.FindViewById<Spinner>(Resource.Id.Assigned);
            List<string> users = new List<string> { "Anna", "Michal" }; 
            var adapter = new ArrayAdapter(assigned.Context, Android.Resource.Layout.SimpleSpinnerItem, users);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            assigned.Adapter = adapter;

            return row;
        }
    }
}