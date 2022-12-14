using System;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    public class OrderTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private Order _order;
            public Order Order
            {
                get
                {
                    Repository.Instance.Refresh(ref _order);
                    return _order;
                }
                private set => _order = value;
            }

            public Label NumberLabel { get; private set; }
            public Label DateLabel { get; private set; }
            public Label PostLabel { get; private set; }

            public void Init(object obj)
            {
                Order order = obj as Order;
                if (order == null)
                {
                    return;
                }
                Order = order;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                NumberLabel = new Label(order.OrderNumber.ToString());
                DateLabel = new Label(order.OrderDate.ToShortDateString());
                PostLabel = new Label(_(order.Post.Name));
                
                box.Add(NumberLabel);
                box.Add(DateLabel);
                box.Add(PostLabel);
                
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
        
        private EntityList<Order, Row> List;
        private OrderMenu Menu = new OrderMenu();
        public OrderTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<Order, Row>) CreateList();
            SetList(List);
            
            List.List.RowSelected += Select;
            
            Menu.FindEmployerButton.Clicked += FindEmployer;
            Menu.SelectEmployerButton.Clicked += SelectEmployer;

            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntry;
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

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            OrderMenu menu = new OrderMenu();
            menu.FindEmployerButton.Sensitive = false;

            menu.SelectEmployerButton.Clicked += (sender, args) =>
            {
                Employees employees = TabManager.SelectDialog(TabName.Employees) as Employees;
                if (employees != null)
                {
                    menu.EmployerLabel.Data["object"] = employees;
                    menu.EmployerLabel.Text = employees.LastName;
                }
            };

            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    Order order = new Order();
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

        private void SaveCurrent(object? sender, EventArgs e)
        {
            if (Menu.SettingData())
            {
                Repository.Instance.Update(Menu.Order);
                Row row = List.GetRow();
                if (row != null && row.Order.Id == Menu.Order.Id)
                {
                    Order order = Menu.Order;
                    row.NumberLabel.Text = order.OrderNumber.ToString();
                    row.DateLabel.Text = order.OrderDate.ToShortDateString();
                    row.PostLabel.Text = _(order.Post.Name);
                }
            }
        }

        private void SelectEmployer(object? sender, EventArgs e)
        {
            Employees employees = TabManager.SelectDialog(TabName.Employees) as Employees;
            if (employees != null)
            {
                Menu.EmployerLabel.Data["object"] = employees;
                Menu.EmployerLabel.Text = employees.LastName;
            }
        }

        private void FindEmployer(object? sender, EventArgs e)
        {
            Employees employees = Menu.EmployerLabel.Data["object"] as Employees;
            if(employees != null)
                TabManager.SelectOnTabById(TabName.Employees,employees.Id);
        }

        private void Select(object o, RowSelectedArgs args)
        {
            Row row = args.Row as Row;
            if (row == null)
            {
                return;
            }

            Box box = (Box) Form.Instance.Builder.GetObject("InfoBox");
            if (box.Children.Length == 0 || box.Children[0].Name != "order_info")
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
            var list = new EntityList<Order, Row>();
            list.AddColumnTitle(_("Number"));
            list.AddColumnTitle(_("Date"));
            list.AddColumnTitle(_("Post"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Reason) like '%{input}%' or " +
            //              (int.TryParse(input, out var _) ? $"e.OrderNumber = {input} or " : "") +
            //              (DateTime.TryParse(input, out var _) ? $"e.OrderDate = '{input}' or " : "") +
            //              $"lower(e.Employees.FirstName) like '%{input}%' or " +
            //              $"lower(e.Employees.LastName) like '%{input}%' or " +
            //              $"lower(e.Employees.Patronymic) like '%{input}%' or " +
            //              $"lower(e.Post.Name) like '%{input}%'";
            string cond = (int.TryParse(input, out var _) ? $"order_number = {input} or " : "") +
                          (DateTime.TryParse(input, out var _) ? $"order_date = '{input}' or " : "") +
                          $"reason ilike '%{input}%'";
            List.SetSearchCondition(cond);
        }
    }

    class OrderMenu : Box
    {

        public Order Order { get; set; }
#pragma warning disable 649
        [UI] public readonly Entry ReasonEntry;
        [UI] public readonly Entry NumberEntry;
        [UI] public readonly ComboBoxText PostComboBox;
        [UI] public readonly Entry DayEntry;
        [UI] public readonly Entry MonthEntry;
        [UI] public readonly Entry YearEntry;
        [UI] public readonly Label EmployerLabel;
        [UI] public readonly Button SelectEmployerButton;
        [UI] public readonly Button FindEmployerButton;
#pragma warning restore 649
        
        public OrderMenu() : this(new Builder("order_menu.glade")){}

        private OrderMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);

            var posts = Repository.Instance.FindByCondition<Post>(new Query(){Table = "Post"});
        foreach (var post in posts)
            {
                PostComboBox.Append(post.Name,_(post.Name));
            }

            NumberEntry.TextInserted += Form.OnlyInt;
            DayEntry.TextInserted += Form.OnlyInt;
            MonthEntry.TextInserted += Form.OnlyInt;
            YearEntry.TextInserted += Form.OnlyInt;
        }

        private void ShowError(string message)
        {
            MessageDialog dialog = new MessageDialog(null,DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,false,null);
            dialog.Text = message;
            dialog.Run();
            dialog.Destroy();
        }
        public void SetOrder(Order order)
        {
            Order = order;
            ReasonEntry.Text = order.Reason;
            NumberEntry.Text = order.OrderNumber.ToString();
            DayEntry.Text = order.OrderDate.Day.ToString();
            MonthEntry.Text = order.OrderDate.Month.ToString();
            YearEntry.Text = order.OrderDate.Year.ToString();
            if (order.Employees != null)
            {
                EmployerLabel.Text = order.Employees.LastName;
                EmployerLabel.Data["object"] = order.Employees;
            }
            else
            {
                EmployerLabel.Text = "none";
                EmployerLabel.Data["object"] = null;
            }

            PostComboBox.ActiveId = order.Post.Name;
        }

        public bool SettingData()
        {
            return SettingData(Order);
        }
        
        public bool SettingData(Order order)
        {
            if(string.IsNullOrEmpty(ReasonEntry.Text))
            {
                ShowError(_("no reason specified"));
                return false;
            }

            if(string.IsNullOrEmpty(NumberEntry.Text))
            {
                ShowError(_("no numberEntry specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(DayEntry.Text))
            {
                ShowError(_("no day specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(MonthEntry.Text))
            {
                ShowError(_("not month specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(YearEntry.Text))
            {
                ShowError(_("no year specified"));
                return false;
            }

            DateTime date;
            if (!DateTime.TryParse($"{YearEntry.Text}-{MonthEntry.Text}-{DayEntry.Text}",out date))
            {
                ShowError(_("incorrect date"));
                return false;
            }
            
            var postQuery = Repository.Instance.FindByCondition<Post>(new Query(){Table = "Post", Count = 1});
            Post post = postQuery[0];
            if (post == null)
            {
                ShowError(_("this post doesn't exist"));
                return false;
            }
            order.Reason = ReasonEntry.Text;
            order.OrderNumber = Convert.ToInt32(NumberEntry.Text);
            order.OrderDate = date;
            order.Employees = EmployerLabel.Data["object"] as Employees;
            order.Post = post;
            
            return true;
        }
    }
}