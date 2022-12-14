using System;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    public class DrinkOrderTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private DrinkOrder _order;

            public DrinkOrder Order
            {
                get
                {
                    Repository.Instance.Refresh(ref _order);
                    return _order;
                }

                set => _order = value;
            }

            public Label DrinkNameLabel { get; private set; }
            public Label WaiterNameLabel { get; private set; }
            public Label TableLabel { get; private set; }
            public void Init(object obj)
            {
                DrinkOrder order = obj as DrinkOrder;
                if (order == null)
                {
                    return;
                }

                _order = order;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;

                DrinkNameLabel = new Label(order.Drink.Name);
                WaiterNameLabel = new Label(order.Waiter.LastName);
                TableLabel = new Label(order.Table.ToString());
                
                box.Add(DrinkNameLabel);
                box.Add(WaiterNameLabel);
                box.Add(TableLabel);
                
                Add(box);
            }

            public int GetId()
            {
                return _order.Id;
            }

            public IEntity GetEntity()
            {
                return Order;
            }
        }

        private readonly EntityList<DrinkOrder, Row> List;
        private readonly DrinkOrderMenu Menu = new DrinkOrderMenu();
        
        public DrinkOrderTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<DrinkOrder, Row>) CreateList();
            SetList(List);
            
            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntry;
            
            List.List.RowSelected += Select;
            
            Menu.DrinkFind.Clicked += FindDrink;
            Menu.DrinkSelect.Clicked += SelectDrink;
            
            Menu.WaiterFind.Clicked += FindWaiter;
            Menu.WaiterSelect.Clicked += SelectWaiter;
            
            tabManager.GetTab(TabName.Employees).OnRemove += WaiterRemoveCallback;
            tabManager.GetTab(TabName.Drink).OnRemove += DrinkRemoveCallback;
        }

        private void DrinkRemoveCallback(IEntity entity)
        {
            foreach (Widget widget in List.GetChildren())
            {
                if (widget is Row row)
                {
                    if (row.Order.Drink.Id == entity.Id)
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

        private void RemoveEntry(object? sender, EventArgs e)
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
                    DrinkOrder order = Menu.Order;
                    row.DrinkNameLabel.Text = order.Drink.Name;
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

        private void SelectDrink(object? sender, EventArgs e)
        {
            if (TabManager.SelectDialog(TabName.Drink) is Drink drink)
            {
                Menu.DrinkLink.Data["object"] = drink;
                Menu.DrinkLink.Text = drink.Name;
            }
        }

        private void FindDrink(object? sender, EventArgs e)
        {
            Drink drink = Menu.DrinkLink.Data["object"] as Drink;
            if (drink == null)
            {
                return;
            }
            TabManager.SelectOnTabById(TabName.Drink,drink.Id);
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

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            DrinkOrderMenu menu = new DrinkOrderMenu();
            menu.DrinkFind.Sensitive = false;
            menu.WaiterFind.Sensitive = false;

            menu.DrinkSelect.Clicked += (sender, args) =>
            {
                if (TabManager.SelectDialog(TabName.Drink) is Drink drink)
                {
                    menu.DrinkLink.Data["object"] = drink;
                    menu.DrinkLink.Text = drink.Name;
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
                    DrinkOrder order = new DrinkOrder();
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

        protected override IList CreateList()
        {
            var list = new EntityList<DrinkOrder, Row>();
            list.AddColumnTitle(_("Drink"));
            list.AddColumnTitle(_("Waiter"));
            list.AddColumnTitle(_("Table"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Drink.Name) like '%{input}%' or " +
            //              $"lower(e.Waiter.FirstName) like '%{input}%' or " +
            //              $"lower(e.Waiter.LastName) like '%{input}%' or " +
            //              $"lower(e.Waiter.Patronymic) like '%{input}%' or " +
            //              (int.TryParse(input, out var _) ? $"e.Table = {input}" : "");
            string cond = (int.TryParse(input, out var _) ? $"table_ = {input}" : "");
            List.SetSearchCondition(cond);
        }
    }

    class DrinkOrderMenu : Box
    {
        public DrinkOrder Order { get; set; }
        
#pragma warning disable 649
        [UI] public readonly Label DrinkLink;
        [UI] public readonly Button DrinkSelect;
        [UI] public readonly Button DrinkFind;

        [UI] public readonly Label WaiterLink;
        [UI] public readonly Button WaiterSelect;
        [UI] public readonly Button WaiterFind;

        [UI] public Entry TableEntry;
        
#pragma warning restore 649
        public DrinkOrderMenu() : this(new Builder("drink_order_menu.glade")){}

        private DrinkOrderMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
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
        
        public bool SettingData(DrinkOrder order)
        {
            if (DrinkLink.Data["object"] == null)
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

            order.Drink = (Drink) DrinkLink.Data["object"];
            order.Waiter = (Employees) WaiterLink.Data["object"];
            order.Table = Convert.ToInt32(TableEntry.Text);
            
            return true;
        }

        public void SetOrder(DrinkOrder order)
        {
            Order = order;

            DrinkLink.Text = order.Drink.Name;
            DrinkLink.Data["object"] = order.Drink;

            WaiterLink.Text = order.Waiter.LastName;
            WaiterLink.Data["object"] = order.Waiter;

            TableEntry.Text = order.Table.ToString();
        }
    }
}