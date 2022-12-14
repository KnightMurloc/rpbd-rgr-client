using System;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;


namespace lab2
{
    public class BankDetailTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private BankDetail _detail;

            public BankDetail Detail
            {
                get
                {
                    Repository.Instance.Refresh(ref _detail);
                    return _detail;
                }

                set => _detail = value;
            }
            
            public Label NameLabel { get; private set; }
            public Label CityLabel { get; private set; }
            public Label ProviderNameLabel { get; private set; }
            
            public void Init(object obj)
            {
                BankDetail detail = obj as BankDetail;
                if (detail == null)
                {
                    return;
                }

                _detail = detail;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                NameLabel = new Label(detail.Name);
                CityLabel = new Label(detail.City.Name);
                if(detail.Provider != null)
                    ProviderNameLabel = new Label(detail.Provider.Name);
                else
                    ProviderNameLabel = new Label("");
                
                box.Add(NameLabel);
                box.Add(CityLabel);
                box.Add(ProviderNameLabel);
                
                Add(box);
            }

            public int GetId()
            {
                return _detail.Id;
            }

            public IEntity GetEntity()
            {
                return Detail;
            }
        }

        private readonly EntityList<BankDetail, Row> List;
        private readonly BankDetailMenu Menu = new BankDetailMenu();

        public BankDetailTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<BankDetail, Row>) CreateList();
            SetList(List);
            
            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            List.List.RowSelected += Select;

            Menu.FindButton.Clicked += FindProvider;
            Menu.SelectButton.Clicked += SelectProvider;
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntity;
            
            tabManager.GetTab(TabName.Provider).OnRemove += OnProviderRemove;
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

            Repository.Instance.Delete(row.Detail);
            List.RemoveRow(row);
        }

        private void OnProviderRemove(IEntity entity)
        {
            foreach (Widget widget in List.GetChildren())
            {
                if (widget is Row row)
                {
                    if (row.Detail.Provider.Id == entity.Id)
                    {
                        List.RemoveRow(row);
                    }
                }
            }
        }

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            BankDetailMenu menu = new BankDetailMenu();
            menu.FindButton.Sensitive = false;

            menu.SelectButton.Clicked += (sender, args) =>
            {
                if (TabManager.SelectDialog(TabName.Provider) is Provider provider)
                {
                    menu.ProviderLink.Data["object"] = provider;
                    menu.ProviderLink.Text = provider.Name;
                }
            };
            
            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    BankDetail detail = new BankDetail();
                    if (menu.SettingData(detail))
                    {
                        Repository.Instance.Create(detail);
                        List.AddEntity(detail);
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

        private void SelectProvider(object? sender, EventArgs e)
        {
            if (TabManager.SelectDialog(TabName.Provider) is Provider provider)
            {
                Menu.ProviderLink.Data["object"] = provider;
                Menu.ProviderLink.Text = provider.Name;
            }
        }
        
        private void FindProvider(object? sender, EventArgs e)
        {
            if (Menu.ProviderLink.Data["object"] is Provider provider)
            {
                TabManager.SelectOnTabById(TabName.Provider,provider.Id);
            }
        }

        private void SaveCurrent(object? sender, EventArgs e)
        {
            if (Menu.SettingData())
            {
                Repository.Instance.Update(Menu.Detail);
                Row row = List.GetRow();
                if (row != null && row.GetId() == Menu.Detail.Id)
                {
                    BankDetail detail = Menu.Detail;
                    row.NameLabel.Text = detail.Name;
                    row.CityLabel.Text = detail.City.Name;
                    if (detail.Provider != null)
                    {
                        row.ProviderNameLabel.Text = detail.Provider.Name;
                    }
                    else
                    {
                        row.ProviderNameLabel.Text = "";
                    }
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
            if (box.Children.Length == 0 || box.Children[0].Name != "bamd_detail_menu")
            {
                if (box.Children.Length != 0)
                {
                    box.Remove(box.Children[0]);
                }
                box.Add(Menu);
            }
            
            Menu.SetDetail(row.Detail);
            box.ShowAll();
        }

        protected override IList CreateList()
        {
            var list = new EntityList<BankDetail, Row>();
            list.AddColumnTitle(_("Name"));
            list.AddColumnTitle(_("City"));
            list.AddColumnTitle(_("Provider Name"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Name) like '%{input}%' or " +
            //              $"lower(e.City) like '%{input}%' or " +
            //              $"lower(e.TIN) like '%{input}%' or " +
            //              $"lower(e.SettlementAccount) like '%{input}%' or " +
            //              $"lower(e.Provider.Name) like '%{input}%'";
            string cond = $"bank_name ilike '%{input}%' or " +
                          $"TIN ilike '%{input}%' or " +
                          $"settlement_account ilike '%{input}%'";
            List.SetSearchCondition(cond);
        }
    }

    class BankDetailMenu : Box
    {
        public BankDetail Detail { get; private set; }
#pragma warning disable 649
        [UI] public readonly Entry NameEntry;
        [UI] public readonly Entry CityEntry;
        [UI] public readonly Entry TinEntry;
        [UI] public readonly Entry SettlementEntry;
        
        [UI] public readonly Label ProviderLink;
        [UI] public readonly Button SelectButton;
        [UI] public readonly Button FindButton;
#pragma warning restore 649

        public BankDetailMenu() : this(new Builder("bank_detail_menu.glade")){}

        private BankDetailMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);
            this.Expand = true;

            TinEntry.TextInserted += Form.OnlyBigInt;
            SettlementEntry.TextInserted += Form.OnlyBigInt;
        }

        private void ShowError(string message)
        {
            MessageDialog dialog = new MessageDialog(null,DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,false,null);
            dialog.Text = message;
            dialog.Run();
            dialog.Destroy();
        }
        
        public void SetDetail(BankDetail detail)
        {
            Detail = detail;

            NameEntry.Text = detail.Name;
            CityEntry.Text = detail.City.Name;
            TinEntry.Text = detail.TIN;
            SettlementEntry.Text = detail.SettlementAccount;

            if (detail.Provider != null)
            {
                ProviderLink.Text = detail.Provider.Name;
                ProviderLink.Data["object"] = detail.Provider;
                FindButton.Sensitive = true;
            }
            else
            {
                ProviderLink.Text = "none";
                ProviderLink.Data["object"] = null;
                FindButton.Sensitive = false;
            }
        }

        public bool SettingData()
        {
            return SettingData(Detail);
        }
        
        public bool SettingData(BankDetail detail)
        {
            if (string.IsNullOrEmpty(NameEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(CityEntry.Text))
            {
                ShowError(_("no city specified"));
                return false;
            }

            if (TinEntry.Text.Length != 10)
            {
                ShowError(_("incorrect TIN"));
                return false;
            }
            
            if (SettlementEntry.Text.Length != 20)
            {
                ShowError(_("incorrect Settlement Account"));
                return false;
            }
            
            // City city = Repository.Instance.FindByCondition<City>($"from City c where c.Name = '{CityEntry.Text}'")[0];
            City city = Repository.Instance.FindByCondition<City>(new Query(){Table = "City"})[0];
            if (city == null)
            {
                city = new City();
                city.Name = CityEntry.Text;
                Repository.Instance.Create(city);
            }
            
            detail.Name = NameEntry.Text;
            detail.City = city;
            detail.TIN = TinEntry.Text;
            detail.SettlementAccount = SettlementEntry.Text;
            detail.Provider = (Provider) ProviderLink.Data["object"];

            return true;
        }
    }
}