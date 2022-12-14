using System;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    public class SnackOrderTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private SnackOrder _order;
            public SnackOrder Order
            {
                get
                {
                    Repository.Instance.Refresh(ref _order);
                    return _order;
                }
                private set => _order = value;
            }

            public Label SnackNameLabel { get; private set; }
            public Label WaiterNameLabel { get; private set; }
            public Label TableLabel { get; set; }
            
            public void Init(object obj)
            {
                SnackOrder order = obj as SnackOrder;
                if (order == null)
                {
                    return;
                }
                Order = order;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                SnackNameLabel = new Label(order.Snack.Name);
                WaiterNameLabel = new Label(order.Waiter.LastName);
                TableLabel = new Label(order.Table.ToString());
                
                box.Add(SnackNameLabel);
                box.Add(WaiterNameLabel);
                box.Add(TableLabel);
                
                Add(box);
            }

            public int GetId()
            {
                return Order.Id;
            }

            public IEntity GetEntity()
            {
                return Order;
            }
        }

        private readonly EntityList<SnackOrder, Row> List;
        private readonly SnackOrderMenu Menu = new SnackOrderMenu();
        public SnackOrderTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<SnackOrder, Row>) CreateList();
            SetList(List);

            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            List.List.RowSelected += Select;
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntity;
            
            Menu.SnackFind.Clicked += FindSnack;
            Menu.WaiterFind.Clicked += FindWaiter;

            Menu.SnackSelect.Clicked += SelectSnack;
            Menu.WaiterSelect.Clicked += SelectWaiter;
            
            tabManager.GetTab(TabName.Snack).OnRemove += OnSnackRemove;
            tabManager.GetTab(TabName.Employees).OnRemove += WaiterRemoveCallback;
        }

        private void OnSnackRemove(IEntity entity)
        {
            foreach (Widget widget in List.GetChildren())
            {
                if (widget is Row row)
                {
                    if (row.Order.Snack.Id == entity.Id)
                    {
                        List.RemoveRow(row);
                    }
                }
            }
        }
        
        private void WaiterRemoveCallback(IEntity entity)
        {
            foreach (Widget widget in List.GetChildren())
            {
                if (widget is Row row)
                {
                    if (row.Order.Waiter.Id == entity.Id)
                    {
                        List.RemoveRow(row);
                    }
                }
            }
        }

        private void RemoveEntity(object? sender, EventArgs e)
        {
            Row row = List.GetRow();
            if (row == null)
            {
                return;
            }

            Box box = (Box) Form.Instance.Builder.GetObject("InfoBox");
            if (box.Children.Length != 0)
            {
                box.Remove(box.Children[0]);
            }

            Repository.Instance.Delete(row.Order);
            List.RemoveRow(row);
        }

        private void SaveCurrent(object? sender, EventArgs e)
        {
            if (Menu.SettingData())
            {
                Repository.Instance.Update(Menu.Order);
                Row row = List.GetRow();
                if (row != null && row.GetId() == Menu.Order.Id)
                {
                    SnackOrder order = Menu.Order;
                    row.SnackNameLabel.Text = order.Snack.Name;
                    row.WaiterNameLabel.Text = order.Waiter.LastName;
                    row.TableLabel.Text = order.Table.ToString();
                }
            }
        }
        
        private void SelectWaiter(object? sender, EventArgs e)
        {
            if (TabManager.SelectDialog(TabName.Employees) is Employees emploer)
            {
                Menu.WaiterLink.Data["object"] = emploer;
                Menu.WaiterLink.Text = emploer.LastName;
            }
        }
        
        private void SelectSnack(object? sender, EventArgs e)
        {
            if (TabManager.SelectDialog(TabName.Snack) is Snack snack)
            {
                Menu.SnackLink.Data["object"] = snack;
                Menu.SnackLink.Text = snack.Name;
            }
        }
        
        private void FindSnack(object? sender, EventArgs e)
        {
            Snack snack = Menu.SnackLink.Data["object"] as Snack;
            if (snack == null)
            {
                return;
            }
            TabManager.SelectOnTabById(TabName.Snack,snack.Id);
        }
        
        private void FindWaiter(object? sender, EventArgs e)
        {
            Employees employer = Menu.WaiterLink.Data["object"] as Employees;
            if (employer == null)
            {
                return;
            }
            TabManager.SelectOnTabById(TabName.Employees,employer.Id);
        }

        private void Select(object o, RowSelectedArgs args)
        {
            Row row = args.Row as Row;
            if (row == null)
            {
                return;
            }

            Box box = (Box) Form.Instance.Builder.GetObject("InfoBox");
            if (box.Children.Length == 0 || box.Children[0].Name != "drink_order_info")
            {
                if (box.Children.Length != 0)
                {
                    box.Remove(box.Children[0]);
                }
                box.Add(Menu);
            }
            
            Menu.SetOrder(row.Order);
            box.ShowAll();
        }
        
        protected override IList CreateList()
        {
            var list = new EntityList<SnackOrder,Row>();
            list.AddColumnTitle(_("Snack"));
            list.AddColumnTitle(_("Waiter"));
            list.AddColumnTitle(_("Table"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Snack.Name) like '%{input}%' or " +
            //              $"lower(e.Waiter.Name) like '%{input}%' or " +
            //              (int.TryParse(input, out var _) ? $"e.Table = {input}" : "");
            string cond = int.TryParse(input, out var _) ? $"table_ = {input}" : "";
            List.SetSearchCondition(cond);
        }

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            SnackOrderMenu menu = new SnackOrderMenu();
            menu.SnackFind.Sensitive = false;
            menu.WaiterFind.Sensitive = false;

            menu.SnackSelect.Clicked += (sender, args) =>
            {
                if (TabManager.SelectDialog(TabName.Snack) is Snack snack)
                {
                    menu.SnackLink.Data["object"] = snack;
                    menu.SnackLink.Text = snack.Name;
                }
            };
            
            menu.WaiterSelect.Clicked += (sender, args) =>
            {
                if (TabManager.SelectDialog(TabName.Employees) is Employees waiter)
                {
                    menu.WaiterLink.Data["object"] = waiter;
                    menu.WaiterLink.Text = waiter.LastName;
                }
            };

            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    SnackOrder order = new SnackOrder();
                    if (menu.SettingData(order))
                    {
                        Repository.Instance.Create(order);
                        List.AddEntity(order);
                    }
                    else
                    {
                        return;
                    }
                }
                dialog.Destroy();
            };
            
            dialog.ShowAll();
        }
    }

    class SnackOrderMenu : Box
    {
        public SnackOrder Order { get; set; }
#pragma warning disable 649
        [UI] public readonly Label SnackLink;
        [UI] public readonly Button SnackSelect;
        [UI] public readonly Button SnackFind;
        
        [UI] public readonly Label WaiterLink;
        [UI] public readonly Button WaiterSelect;
        [UI] public readonly Button WaiterFind;

        [UI] public readonly Entry TableEntry;
#pragma warning restore 649
        
        public SnackOrderMenu() : this(new Builder("snack_order_menu.glade")){}

        private SnackOrderMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);

            TableEntry.TextInserted += Form.OnlyInt;
        }
        
        private void ShowError(string message)
        {
            MessageDialog dialog = new MessageDialog(null,DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,false,null);
            dialog.Text = message;
            dialog.Run();
            dialog.Destroy();
        }

        public bool SettingData()
        {
            return SettingData(Order);
        }
        
        public bool SettingData(SnackOrder order)
        {
            if (SnackLink.Data["object"] == null)
            {
                ShowError(_("no drink specified"));
                return false;
            }

            if (WaiterLink.Data["object"] == null)
            {
                ShowError(_("no waiter specified"));
                return false;
            }

            if (string.IsNullOrEmpty(TableEntry.Text))
            {
                ShowError(_("no table specified"));
                return false;
            }

            order.Snack = (Snack) SnackLink.Data["object"];
            order.Waiter = (Employees) WaiterLink.Data["object"];
            order.Table = Convert.ToInt32(TableEntry.Text);
            
            return true;
        }

        public void SetOrder(SnackOrder order)
        {
            Order = order;
            
            SnackLink.Text = order.Snack.Name;
            SnackLink.Data["object"] = order.Snack;
            
            WaiterLink.Text = order.Waiter.LastName;
            WaiterLink.Data["object"] = order.Waiter;

            TableEntry.Text = order.Table.ToString();
        }
    }
}