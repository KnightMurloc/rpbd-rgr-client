using System;
using System.Collections.Generic;
using Gtk;
using lab2.Models;
using UI = Gtk.Builder.ObjectAttribute;
using static lab2.locale.NGettextShortSyntax;

namespace lab2
{
    public class EmployeesTab : Tab
    {
        private readonly EmployeerMenu Menu = new EmployeerMenu();
        
        class Row : ListBoxRow, IRow
        {
            // public Employees Employee { get; private set; }
            private Employees _employees;

            public Employees Employee
            {
                get
                {
                    Repository.Instance.Refresh(ref _employees);
                    return _employees;
                }

                set => _employees = value;
            }

            public Label FirstNameLabel { get; private set; }
            public Label LastNameLabel { get; private set; }
            public Label PatronymicLabel { get; private set; }
            public Label AddressLabel { get; private set; }
            public Label BirthDateLabel { get; private set; }
            public Label SalaryLabel { get; private set; }
            public Label PostLabel { get; private set; }
            public void Init(object obj)
            {
                Employees employee = obj as Employees;
                if (employee == null)
                {
                    return;
                }
                
                Employee = employee;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
                
                FirstNameLabel = new Label(employee.FirstName);
                LastNameLabel = new Label(employee.LastName);
                PatronymicLabel = new Label(employee.Patronymic);
                AddressLabel = new Label(employee.Address.Name);
                BirthDateLabel = new Label(employee.BirthDate.ToShortDateString());
                SalaryLabel = new Label(employee.Salary.ToString("F"));
                PostLabel = new Label(_(employee.Post.Name));

                box.Add(FirstNameLabel);
                box.Add(LastNameLabel);
                box.Add(PatronymicLabel);
                box.Add(AddressLabel);
                box.Add(BirthDateLabel);
                box.Add(SalaryLabel);
                box.Add(PostLabel);
                
                Add(box);
            }

            public int GetId()
            {
                return Employee.Id;
            }

            public IEntity GetEntity()
            {
                return Employee;
            }
        }
        
        private EntityList<Employees, Row> List;
        
        public EmployeesTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<Employees, Row>) CreateList();
            SetList(List);
            
            List.List.RowSelected += Select;
            
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
            OnRemove.Invoke(row.Employee);
            Repository.Instance.Delete(row.Employee);
            List.RemoveRow(row);
        }

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            EmployeerMenu menu = new EmployeerMenu();

            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    Employees employees = new Employees();
                    if (menu.SettingData(ref employees))
                    {
                        Repository.Instance.Create(employees);
                        List.AddEntity(employees);
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
                Row row = List.GetRow();
                if (row != null && row.Employee.Id == Menu.Employees.Id)
                {
                    Employees employees = Menu.Employees;
                    row.FirstNameLabel.Text = employees.FirstName;
                    row.LastNameLabel.Text = employees.LastName;
                    row.PatronymicLabel.Text = employees.Patronymic;
                    row.AddressLabel.Text = employees.Address.Name;
                    row.BirthDateLabel.Text = employees.BirthDate.ToShortDateString();
                    row.SalaryLabel.Text = employees.Salary.ToString("F");
                    row.PostLabel.Text = _(employees.Post.Name);
                }

                Repository.Instance.Update(Menu.Employees);
            }
        }

        private void Select(object sender, RowSelectedArgs args)
        {
            Row row = args.Row as Row;
            if (row == null)
            {
                return;
            }

            Box box = (Box) Form.Instance.Builder.GetObject("InfoBox");
            if (box.Children.Length == 0 || box.Children[0].Name != "empoyeer_menu")
            {
                if (box.Children.Length != 0)
                {
                    box.Remove(box.Children[0]);
                }
                box.Add(Menu);
            }
            
            Menu.SetEmployer(row.Employee);
            box.ShowAll();
        }

        protected override IList CreateList()
        {
            var list = new EntityList<Employees,Row>();
            list.AddColumnTitle(_("first name"));
            list.AddColumnTitle(_("second name"));
            list.AddColumnTitle(_("Patronymic"));
            list.AddColumnTitle(_("Address"));
            list.AddColumnTitle(_("Birthday"));
            list.AddColumnTitle(_("Salary"));
            list.AddColumnTitle(_("Post"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hsq = $"lower(e.FirstName) like '%{input}%' or " +
            //              $"lower(e.LastName) like '%{input}%' or " +
            //              $"lower(e.Patronymic) like '%{input}%' or " +
            //              $"lower(e.Address) like '%{input}%' or " +
            //              (DateTime.TryParse(input, out var _) ? $"e.BirthDate = '{input}' or " : "") +
            //              (float.TryParse(input, out var _) ? $"e.Salary = {input} or " : "") +
            //              $"lower(e.Post.Name) like '%{_(input)}%'";

            string cond = $"first_name ilike '%{input}%' or " +
                          $"last_name ilike '%{input}%' or " +
                          (DateTime.TryParse(input, out var _) ? $"birth_date = '{input}' or " : "") +

                          (float.TryParse(input, out var _) ? $"salary = {input} or " : "") +
                          $"patronymic ilike '%{input}%'";

                          List.SetSearchCondition(cond);
        }
    }

    class EmployeerMenu : Box
    {
        public Employees Employees { get; private set; }
        
#pragma warning disable 649
        [UI] public readonly Entry FirstNameEntry;
        [UI] public readonly Entry LastNameEntry;
        [UI] public readonly Entry PatronymicEntry;
        [UI] public readonly Entry AddressEntry;
        [UI] public readonly Entry DayEntry;
        [UI] public readonly Entry MonthEntry;
        [UI] public readonly Entry YearEntry;
        [UI] public readonly Entry SalaryEntry;
        [UI] public readonly ComboBoxText PostComboBox;
#pragma warning restore 649
        public EmployeerMenu() : this(new Builder("employeer_menu.glade")){}

        private EmployeerMenu(Builder builder) : base(builder.GetRawOwnedObject("employees_info"))
        {
            builder.Autoconnect(this);

            var posts = Repository.Instance.FindByCondition<Post>(new Query(){Table = "Post"});
            foreach (var post in posts)
            {
                PostComboBox.Append(post.Name,_(post.Name));
            }

            DayEntry.TextInserted += Form.OnlyInt;
            MonthEntry.TextInserted += Form.OnlyInt;
            YearEntry.TextInserted += Form.OnlyInt;
            SalaryEntry.TextInserted += Form.OnlyFloat;
        }

        public void SetEmployer(Employees employees)
        {
            Employees = employees;
            FirstNameEntry.Text = employees.FirstName;
            LastNameEntry.Text = employees.LastName;
            PatronymicEntry.Text = employees.Patronymic;
            AddressEntry.Text = employees.Address.Name;
            DayEntry.Text = employees.BirthDate.Day.ToString();
            MonthEntry.Text = employees.BirthDate.Month.ToString();
            YearEntry.Text = employees.BirthDate.Year.ToString();
            SalaryEntry.Text = employees.Salary.ToString("F");
            PostComboBox.ActiveId = employees.Post.Name;
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
            var employees = Employees;
            return SettingData(ref employees);
        }
        
        public bool SettingData(ref Employees employees)
        {
            if(string.IsNullOrEmpty(FirstNameEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }

            if (string.IsNullOrEmpty(LastNameEntry.Text))
            {
                ShowError(_("no last name specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(AddressEntry.Text))
            {
                ShowError(_("no address specified"));
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

            if (string.IsNullOrEmpty(SalaryEntry.Text))
            {
                ShowError(_("no salary specified"));
                return false;
            }

            Console.WriteLine(PostComboBox.ActiveId);
            // var postQuery = Repository.Instance.FindByCondition<Post>($"from Post p where p.Name = '{PostComboBox.ActiveId}'", 1);
            Post post = null;
            {
                Query query = new Query() {Table = "Post", Count = 1, Condition = $"name = '{PostComboBox.ActiveId}'"};
                var result = Repository.Instance.FindByCondition<Post>(query);
                if (result.Count != 0)
                {
                    post = result[0];
                }
            }
            if (post == null)
            {
                ShowError(_("this post doesn't exist"));
                return false;
            }
            
            // Repository.Instance.FindByCondition<Address>($"from Address a where a.Name = '{AddressEntry.Text}'")[0];
            Address address = null;
            {
                Query query = new Query() {Table = "Address", Count = 1, Condition = $"name = '{AddressEntry.Text}'"};
                var result = Repository.Instance.FindByCondition<Address>(query);
                if (result.Count != 0)
                {
                    address = result[0];
                }
            }
            if (address == null)
            {
                address = new Address();
                address.Name = AddressEntry.Text;
                Repository.Instance.Create(address);
            }
            
            employees.FirstName = FirstNameEntry.Text;
            employees.LastName = LastNameEntry.Text;
            employees.Patronymic = PatronymicEntry.Text;
            employees.Address = address;
            employees.BirthDate = date;
            employees.Salary = Convert.ToSingle(SalaryEntry.Text);
            employees.Post = post;

            return true;
        }
    }
}