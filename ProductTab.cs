using System;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    public class ProductTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private Product _product;

            public Product Product
            {
                get
                {
                    Repository.Instance.Refresh(ref _product);
                    return _product;
                }
            }
            
            public Label NameLabel { get; private set; }
            public Label IngredientNameLabel { get; private set; }
            public Label PriceLabel { get; private set; }
            public Label ProviderNameLabel { get; private set; }
            public void Init(object obj)
            {
                Product product = obj as Product;
                if (product == null)
                {
                    return;
                }

                _product = product;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                NameLabel = new Label(product.Name);
                IngredientNameLabel = new Label(product.Ingredient.Name);
                PriceLabel = new Label(product.Price.ToString("F"));
                ProviderNameLabel = new Label();
                if (product.Provider != null)
                {
                    ProviderNameLabel.Text = product.Provider.Name;
                }
                    
                
                box.Add(NameLabel);
                box.Add(IngredientNameLabel);
                box.Add(PriceLabel);
                box.Add(ProviderNameLabel);
                
                Add(box);
            }

            public int GetId()
            {
                return _product.Id;
            }

            public IEntity GetEntity()
            {
                return Product;
            }
        }

        private readonly EntityList<Product, Row> List; 
        private readonly ProductMenu Menu = new ProductMenu();
        public ProductTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<Product, Row>) CreateList();
            SetList(List);
            
            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            List.List.RowSelected += Select;
            Menu.IngredientFind.Clicked += FindIngredient;
            Menu.IngredientSelect.Clicked += SelectIngredient;
            Menu.ProviderFind.Clicked += FindProvider;
            Menu.ProviderSelect.Clicked += ProviderSelect;
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntry;

            tabManager.GetTab(TabName.Ingredients).OnRemove += RemoveIngredientCallback;
        }

        private void ProviderSelect(object? sender, EventArgs e)
        {
            if (TabManager.SelectDialog(TabName.Provider) is Provider provider)
            {
                Menu.ProviderLabel.Data["object"] = provider;
                Menu.ProviderLabel.Text = provider.Name;
            }
        }

        private void RemoveIngredientCallback(IEntity entity)
        {
            foreach (Widget widget in List.GetChildren())
            {
                if (widget is Row row)
                {
                    if (row.Product.Ingredient.Id == entity.Id)
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

            Repository.Instance.Delete(row.Product);
            List.RemoveRow(row);
        }

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);

            ProductMenu menu = new ProductMenu();
            menu.IngredientFind.Sensitive = false;
            menu.ProviderFind.Sensitive = false;

            menu.IngredientSelect.Clicked += (sender, args) =>
            {
                if (TabManager.SelectDialog(TabName.Ingredients) is Ingredient ingredient)
                {
                    menu.IngredientLabel.Data["object"] = ingredient;
                    menu.IngredientLabel.Text = ingredient.Name;
                }
            };

            menu.ProviderSelect.Clicked += (sender, args) =>
            {
                if (TabManager.SelectDialog(TabName.Provider) is Provider provider)
                {
                    menu.ProviderLabel.Data["object"] = provider;
                    menu.ProviderLabel.Text = provider.Name;
                }
            };
            
            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    Product product = new Product();
                    if (menu.SettingData(product))
                    {
                        Repository.Instance.Create(product);
                        List.AddEntity(product);
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
                Repository.Instance.Update(Menu.Product);
                Row row = List.GetRow();
                if (row != null && row.GetId() == Menu.Product.Id)
                {
                    Product product = Menu.Product;

                    row.NameLabel.Text = product.Name;
                    row.IngredientNameLabel.Text = product.Ingredient.Name;
                    row.PriceLabel.Text = product.Price.ToString("F");
                    row.ProviderNameLabel.Text = product.Provider != null ? product.Provider.Name : "";
                }
            }
        }

        private void SelectIngredient(object? sender, EventArgs e)
        {
            Ingredient ingredient = TabManager.SelectDialog(TabName.Ingredients) as Ingredient;
            if (ingredient != null)
            {
                Menu.IngredientLabel.Data["object"] = ingredient;
                Menu.IngredientLabel.Text = ingredient.Name;
            }
        }

        private void FindIngredient(object? sender, EventArgs e)
        {
            Ingredient ingredient = Menu.IngredientLabel.Data["object"] as Ingredient;
            if (ingredient == null)
            {
                return;
            }
            TabManager.SelectOnTabById(TabName.Ingredients,ingredient.Id);
        }

        private void FindProvider(object? sender, EventArgs e)
        {
            Provider provider = Menu.ProviderLabel.Data["object"] as Provider;
            if (provider == null)
            {
                return;
            }
            TabManager.SelectOnTabById(TabName.Provider,provider.Id);
        }
        private void Select(object o, RowSelectedArgs args)
        {
            Row row = args.Row as Row;
            if (row == null)
            {
                return;
            }

            Box box = (Box) Form.Instance.Builder.GetObject("InfoBox");
            if (box.Children.Length == 0 || box.Children[0].Name != "product_menu")
            {
                if (box.Children.Length != 0)
                {
                    box.Remove(box.Children[0]);
                }
                box.Add(Menu);
            }
            
            Menu.SetProduct(row.Product);
            box.ShowAll();
        }

        protected override IList CreateList()
        {
            var list = new EntityList<Product,Row>();
            list.AddColumnTitle(_("Name"));
            list.AddColumnTitle(_("Ingredient"));
            list.AddColumnTitle(_("Price"));
            list.AddColumnTitle(_("Provider"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Ingredient.Name) like '%{input}%' or " +
            //              (float.TryParse(input, out var _) ? $"e.Price = {input} or " : "") +
            //              $"lower(e.DeliveryTerms) like '%{input}%' or " +
            //              $"lower(e.PaymentTerms) like '%{input}%' or " +
            //              $"lower(e.Provider.Name) like '%{input}%' or " +
            //              $"lower(e.Name) like '%{input}%'";
            string cond = (float.TryParse(input, out var _) ? $"e.Price = {input} or " : "") +
                         $"delivery_terms ilike '%{input}%' or " +
                         $"payment_terms ilike '%{input}%' or " +
                         $"name ilike '%{input}%'";
                         
            List.SetSearchCondition(cond);
        }
    }

    class ProductMenu : Box
    {
        public Product Product { get; set; }
        
#pragma warning disable 649
        [UI] public readonly Label IngredientLabel;
        [UI] public readonly Button IngredientFind;
        [UI] public readonly Button IngredientSelect;

        [UI] public readonly Entry PriceEntry;
        [UI] public readonly Entry DeliveryTermsEntry;
        [UI] public readonly Entry PaymentTermsEntry;
        
        [UI] public readonly Label ProviderLabel;
        [UI] public readonly Button ProviderFind;
        [UI] public readonly Button ProviderSelect;

        [UI] public readonly Entry NameEntry;
#pragma warning restore 649
        public ProductMenu() : this(new Builder("product_menu.glade")){}

        private ProductMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);

            PriceEntry.TextInserted += Form.OnlyFloat;
        }

        public void SetProduct(Product product)
        {
            Product = product;
            IngredientLabel.Text = product.Ingredient.Name;
            IngredientLabel.Data["object"] = product.Ingredient;

            PriceEntry.Text = product.Price.ToString("F");
            DeliveryTermsEntry.Text = product.DeliveryTerms;
            PaymentTermsEntry.Text = product.PaymentTerms;

            if (product.Provider != null)
            {
                ProviderLabel.Text = product.Provider.Name;
                ProviderLabel.Data["object"] = product.Provider;
                ProviderFind.Sensitive = true;
            }
            else
            {
                ProviderLabel.Text = "none";
                ProviderLabel.Data["object"] = null;
                ProviderFind.Sensitive = false;
            }

            NameEntry.Text = product.Name;
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
            return SettingData(Product);
        }
        
        public bool SettingData(Product product)
        {
            if (!(IngredientLabel.Data["object"] is Ingredient))
            {
                ShowError(_("no ingredient specified"));
                return false;
            }

            if (string.IsNullOrEmpty(PriceEntry.Text))
            {
                ShowError(_("no price specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(DeliveryTermsEntry.Text))
            {
                ShowError(_("no Delivery Terms specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(PaymentTermsEntry.Text))
            {
                ShowError(_("no Payment Terms specified"));
                return false;
            }

            if (ProviderLabel.Data["object"] != null && !(ProviderLabel.Data["object"] is Provider))
            {
                ShowError(_("provider set error"));
                return false;
            }
            
            product.Ingredient = IngredientLabel.Data["object"] as Ingredient;
            product.Price = Convert.ToSingle(PriceEntry.Text);
            product.DeliveryTerms = DeliveryTermsEntry.Text;
            product.PaymentTerms = PaymentTermsEntry.Text;
            product.Provider = ProviderLabel.Data["object"] as Provider;
            product.Name = NameEntry.Text;

            return true;
        }
    }
}