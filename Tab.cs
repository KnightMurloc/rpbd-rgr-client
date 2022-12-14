using System;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;

namespace lab2
{
    public delegate void OnRemove(IEntity entity);
    public abstract class Tab : Box
    {
        protected readonly TabManager TabManager;
        protected readonly Button AddButton;
        protected readonly Button RemoveButton;

        private IList List;
        public OnRemove OnRemove { get; set; }

        protected abstract IList CreateList();
        protected abstract void Search(string input);

        protected readonly SearchEntry SearchEntry;
        protected readonly Button SearchCancel;

        protected Tab(TabManager tabManager) : base(Orientation.Vertical, 0)
        {
            TabManager = tabManager;

            //Create top panel

            Box box = new Box(Orientation.Horizontal, 20);
            AddButton = new Button("gtk-add");
            box.Add(AddButton);
            RemoveButton = new Button("gtk-delete");
            box.Add(RemoveButton);

            Box bottom = new Box(Orientation.Horizontal, 0);
            SearchEntry = new SearchEntry {Hexpand = true};

            SearchEntry.Activated += StartSearch;

            SearchCancel = new Button("gtk-cancel");
            SearchCancel.Clicked += CancelSearch;
            bottom.Add(SearchEntry);
            bottom.Add(SearchCancel);
            
            Add(box);
            PackEnd(bottom,false,false,0);
        }

        private void CancelSearch(object? sender, EventArgs e)
        {
            List.CancelSearch();
            List.FillList();
        }

        private void StartSearch(object? sender, EventArgs e)
        {
            string input = "";
            foreach (char c in SearchEntry.Text.ToLower())
            {
                if (c == '\'')
                {
                    input += '\'';
                    input += '\'';
                }
                else
                {
                    input += c;
                }
            }
            
            Search(input);
            List.FillList();
        }

        protected void SetList(IList list)
        {
            List = list;
            Box box = (Box) list;
            Add(box);
        }
        
        public void SelectById(int id)
        {
            List.FillList();

            do
            {
                foreach (Widget widget in List.GetChildren())
                {
                    IRow row = widget as IRow;
                    if (row == null)
                    {
                        continue;
                    }
                    if (row.GetId() == id)
                    {
                        List.SelectRow(widget);
                        return;
                    }
                }
            } while (List.ScrollDown());
        }

        public object SelectDialog()
        {
            Dialog dialog = new Dialog();
            dialog.SetDefaultSize(500,500);
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);

            var list = CreateList();
            
            Box box = list as Box;
            
            ((Container) dialog.Children[0]).Add(box);

            box.ShowAll();

            if (dialog.Run() == (int) ResponseType.Ok)
            {
                dialog.Destroy();

                return list.getSelected().GetEntity();
            }
            
            dialog.Destroy();

            return null;
        }
    }
}