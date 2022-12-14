using System;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    public class ProviderTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private Provider _provider;

            public Provider Provider
            {
                get
                {
                    Repository.Instance.Update(_provider);
                    return _provider;
                }

                set => _provider = value;
            }

            public Label NameLabel { get; private set; }
            public Label PhoneNumberLabel { get; private set; }
            public Label FaxLabel { get; private set; }
            public Label EmailLabel { get; private set; }
            

            public void Init(object obj)
            {
                Provider provider = obj as Provider;
                if (provider == null)
                {
                    return;
                }

                _provider = provider;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                NameLabel = new Label(provider.Name);
                PhoneNumberLabel = new Label(provider.PhoneNumber);
                FaxLabel = new Label(provider.Fax);
                EmailLabel = new Label(provider.Email);
                
                box.Add(NameLabel);
                box.Add(PhoneNumberLabel);
                box.Add(FaxLabel);
                box.Add(EmailLabel);
                
                Add(box);
            }

            public int GetId()
            {
                return _provider.Id;
            }

            public IEntity GetEntity()
            {
                return Provider;
            }
        }

        private readonly EntityList<Provider, Row> List;
        private readonly ProviderMenu Menu = new ProviderMenu();
        
        public ProviderTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<Provider, Row>) CreateList();
            SetList(List);
            
            List.List.RowSelected += Select;
            
            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntity;
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

            OnRemove.Invoke(row.Provider);
            Repository.Instance.Delete(row.Provider);
            List.RemoveRow(row);
        }

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            ProviderMenu menu = new ProviderMenu();


            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    Provider provider = new Provider();
                    if (menu.SettingData(provider))
                    {
                        Repository.Instance.Create(provider);
                        List.AddEntity(provider);
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
                Repository.Instance.Update(Menu.Provider);
                Row row = List.GetRow();
                if (row != null && row.GetId() == Menu.Provider.Id)
                {
                    Provider provider = Menu.Provider;
                    row.NameLabel.Text = provider.Name;
                    row.PhoneNumberLabel.Text = provider.PhoneNumber;
                    row.FaxLabel.Text = provider.Fax;
                    row.EmailLabel.Text = provider.Email;
                }
            }
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
            
            Menu.SetProvider(row.Provider);
            box.ShowAll();
        }

        protected override IList CreateList()
        {
            var list = new EntityList<Provider,Row>();
            list.AddColumnTitle(_("Name"));
            list.AddColumnTitle(_("Phone Number"));
            list.AddColumnTitle(_("Fax"));
            list.AddColumnTitle(_("Email"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Name) like '%{input}%' or " +
            //              $"lower(e.PostAddress) like '%{input}%' or " +
            //              $"lower(e.PhoneNumber) like '%{input}%' or " +
            //              $"lower(e.Fax) like '%{input}%' or " +
            //              $"lower(e.Email) like '%{input}%'";

            string cond = $"name ilike '%{input}%' or " +
                          $"phone_number ilike '%{input}%' or " +
                          $"fax ilike '%{input}%' or " +
                          $"email ilike '%{input}%'";
            List.SetSearchCondition(cond);
        }
    }


    class ProviderMenu : Box
    {
        public Provider Provider { get; set; }
#pragma warning disable 649
        [UI] public readonly Entry NameEntry;
        [UI] public readonly Entry AddressEntry;
        [UI] public readonly Entry NumberEntry;
        [UI] public readonly Entry FaxEntry;
        [UI] public readonly Entry EmailEntry;
#pragma warning restore 649

        public ProviderMenu() : this(new Builder("provider_menu.glade")){}

        private ProviderMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);
        }

        public void SetProvider(Provider provider)
        {
            Provider = provider;

            NameEntry.Text = provider.Name;
            AddressEntry.Text = provider.PostAddress.Name;
            NumberEntry.Text = provider.PhoneNumber;
            FaxEntry.Text = provider.Fax;
            EmailEntry.Text = provider.Email;
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
            return SettingData(Provider);
        }
        
        public bool SettingData(Provider provider)
        {
            if (string.IsNullOrEmpty(NameEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }
            if (string.IsNullOrEmpty(AddressEntry.Text))
            {
                ShowError(_("no address specified"));
                return false;
            }
            if (string.IsNullOrEmpty(NumberEntry.Text))
            {
                ShowError(_("no phone number specified"));
                return false;
            }
            if (string.IsNullOrEmpty(FaxEntry.Text))
            {
                ShowError(_("no fax specified"));
                return false;
            }
            if (string.IsNullOrEmpty(EmailEntry.Text))
            {
                ShowError(_("no email specified"));
                return false;
            }

            Address address = null;

            {
                Query query = new Query(){Table = "Address", Count = 1, Condition = $"name = '{AddressEntry.Text}'"};
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
            
            // Address address =
                // Repository.Instance.FindByCondition<Address>($"from Address a where a.Name = '{AddressEntry.Text}'")[0];
            // if (address == null)
            // {
                // address = new Address();
                // address.Name = AddressEntry.Text;
                // Repository.Instance.Create(address);
            // }

            provider.Name = NameEntry.Text;
            provider.PostAddress = address;
            provider.PhoneNumber = NumberEntry.Text;
            provider.Fax = FaxEntry.Text;
            provider.Email = EmailEntry.Text;

            return true;
        }
    }
}