using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    public class DrinkTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private Drink _drink;

            public Drink Drink
            {
                get
                {
                    Repository.Instance.Refresh(ref _drink);
                    return _drink;
                }

                set => _drink = value;
            }

            public Label NameLabel { get; private set; }
            public Label StrengthLabel { get; private set; }
            public Label SizeLabel { get; private set; }
            public Label ContainerLabel { get; set; }
            
            public void Init(object obj)
            {
                Drink drink = obj as Drink;
                if (drink == null)
                {
                    return;
                }

                _drink = drink;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                NameLabel = new Label(drink.Name);
                StrengthLabel = new Label(drink.Strength.ToString());
                SizeLabel = new Label(drink.Size.ToString());
                ContainerLabel = new Label(drink.Container);
                
                box.Add(NameLabel);
                box.Add(StrengthLabel);
                box.Add(SizeLabel);
                box.Add(ContainerLabel);
                
                
                Add(box);
            }

            public int GetId()
            {
                return _drink.Id;
            }

            public IEntity GetEntity()
            {
                return Drink;
            }
        }

        private readonly EntityList<Drink, Row> List;
        private readonly DrinkMenu Menu = new DrinkMenu();
        
        public DrinkTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<Drink, Row>) CreateList();
            SetList(List);
            
            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntity;
            
            List.List.RowSelected += Select;
            
            Menu.AddButton.Clicked += AddIngredient;
            Menu.RemoveButton.Clicked += RemoveIngredient;
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
            OnRemove.Invoke(row.Drink);
            Repository.Instance.Delete(row.Drink);
            List.RemoveRow(row);
        }

        private void RemoveIngredient(object? sender, EventArgs e)
        {
            Widget row = Menu.IngredientList.SelectedRow;
            if (row == null)
            {
                return;
            }
            
            Menu.IngredientList.Remove(row);
        }
        
        private void AddIngredient(object? sender, EventArgs e)
        {
            if (TabManager.SelectDialog(TabName.Ingredients) is Ingredient ingredient)
            {
                Dialog dialog = new Dialog();
                dialog.AddButton("OK", ResponseType.Ok);
                dialog.AddButton(_("Cancel"), ResponseType.Cancel);
                Entry entry = new Entry();
                ((Container) dialog.Children[0]).Add(entry);
                dialog.ShowAll();
                entry.TextInserted += Form.OnlyInt;
                if (dialog.Run() != (int) ResponseType.Ok)
                {
                    dialog.Destroy();
                    return;
                }

                ListBoxRow row = new ListBoxRow();
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
                    
                Label name = new Label(ingredient.Name);
                Label size = new Label(entry.Text);
                box.Add(name);
                box.Add(size);

                row.Data["object"] = ingredient;
                row.Data["count"] = Convert.ToInt32(entry.Text);
                row.Add(box);
                Menu.IngredientList.Add(row);
                Menu.IngredientList.ShowAll();
                dialog.Destroy();
            }
        }

        private void SaveCurrent(object? sender, EventArgs e)
        {
            if (Menu.SettingData())
            {
                Repository.Instance.Update(Menu.Drink);
                Row row = List.GetRow();
                if (row != null && row.GetId() == Menu.Drink.Id)
                {
                    Drink drink = Menu.Drink;
                    row.NameLabel.Text = drink.Name;
                    row.StrengthLabel.Text = drink.Strength.ToString();
                    row.ContainerLabel.Text = drink.Container;
                    row.SizeLabel.Text = drink.Size.ToString();
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
            if (box.Children.Length == 0 || box.Children[0].Name != "dring_menu")
            {
                if (box.Children.Length != 0)
                {
                    box.Remove(box.Children[0]);
                }
                box.Add(Menu);
            }
            
            Menu.SetDrink(row.Drink);
            box.ShowAll();
        }

        private void CreateEntity(object? sender, EventArgs e)
        { 
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            DrinkMenu menu = new DrinkMenu();

            menu.AddButton.Clicked += (o, args) =>
            {
                if (TabManager.SelectDialog(TabName.Ingredients) is Ingredient ingredient)
                {
                    Dialog dialog = new Dialog();
                    dialog.AddButton("OK", ResponseType.Ok);
                    dialog.AddButton(_("Cancel"), ResponseType.Cancel);
                    Entry entry = new Entry();
                    ((Container) dialog.Children[0]).Add(entry);
                    dialog.ShowAll();
                    entry.TextInserted += Form.OnlyInt;
                    if (dialog.Run() != (int) ResponseType.Ok)
                    {
                        dialog.Destroy();
                        return;
                    }

                    ListBoxRow row = new ListBoxRow();
                    Box box = new Box(Orientation.Horizontal,0);
                    box.Homogeneous = true;
                    
                    Label name = new Label(ingredient.Name);
                    Label size = new Label(entry.Text);
                    box.Add(name);
                    box.Add(size);

                    row.Data["object"] = ingredient;
                    row.Data["count"] = Convert.ToInt32(entry.Text);
                    row.Add(box);
                    menu.IngredientList.Add(row);
                    menu.IngredientList.ShowAll();
                    dialog.Destroy();
                }
            };

            menu.RemoveButton.Clicked += (o, args) =>
            {
                Widget row = menu.IngredientList.SelectedRow;
                if (row == null)
                {
                    return;
                }
            
                menu.IngredientList.Remove(row);
            };
            
            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    Drink drink = new Drink();
                    if (menu.SettingData(drink))
                    {
                        Repository.Instance.Create(drink);
                        List.AddEntity(drink);
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
            var list = new EntityList<Drink, Row>();
            list.AddColumnTitle(_("Name"));
            list.AddColumnTitle(_("Strength"));
            list.AddColumnTitle(_("Size"));
            list.AddColumnTitle(_("Container"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Name) like '%{input}%' or " +
            //              (int.TryParse(input, out var _) ? $"e.Strength = {input} or " : "") +
            //              (int.TryParse(input, out var _) ? $"e.Size = {input} or " : "") +
            //              $"lower(e.Container) like '%{input}%'";
            string cond = $"name ilike '%{input}%' or " +
                          (int.TryParse(input, out var _) ? $"strength = {input} or " : "") +
                          (int.TryParse(input, out var _) ? $"size = {input} or " : "") +
                          $"container ilike '%{input}%'";

            List.SetSearchCondition(cond);
        }    
    }
    
    class DrinkMenu : Box
    {
        public Drink Drink { get; set; }

#pragma warning disable 649
        [UI] public readonly Entry NameEntry;
        [UI] public readonly Entry StrengthEntry;
        [UI] public readonly Entry SizeEntry;
        [UI] public readonly Entry ContainerEntry;
        
        [UI] public readonly Button AddButton;
        [UI] public readonly Button RemoveButton;
        [UI] public readonly ListBox IngredientList;
#pragma warning restore 649

        public DrinkMenu() : this(new Builder("drink_menu.glade")){}

        private DrinkMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);
            this.Expand = true;

            SizeEntry.TextInserted += Form.OnlyInt;
            StrengthEntry.TextInserted += Form.OnlyInt;
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
            return SettingData(Drink);
        }
        private List<T> different<T, U>(List<T> a, List<U> b, Func<U, T,bool> cmp)
        {
            List<T> result = new List<T>();
            foreach (T obj1 in a)
            {
                bool find = false;
                foreach (U obj2 in b)
                {
                    if (cmp(obj2,obj1))
                    {
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    result.Add(obj1);
                }
            }

            return result;
        }
        public bool SettingData(Drink drink)
        {
            if (string.IsNullOrEmpty(NameEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }

            if (string.IsNullOrEmpty(StrengthEntry.Text))
            {
                ShowError(_("no Strength specified"));
                return false;
            }

            if (string.IsNullOrEmpty(ContainerEntry.Text))
            {
                ShowError(_("no Container specified"));
                return false;
            }
            
            if (string.IsNullOrEmpty(SizeEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }
            
            var newList = IngredientList.Children;
            var oldList = drink.Ingredients;
            
            // var add = newList.Where(p => !oldList.Any(l => (p.Data["object"] as Ingredient).Id == l.Ingredient.Id));
            // var remove = oldList.Where(p => !newList.Any(l => p.Ingredient.Id == (l.Data["object"] as Ingredient).Id));
            
            var add = different(newList.ToList(), oldList, (recipes, widget) => recipes.Ingredient.Id == ((Ingredient) widget.Data["object"]).Id);
            var remove = different(oldList, newList.ToList(), (widget, recipes) =>
            {
                return recipes.Ingredient.Id == ((Ingredient) widget.Data["object"]).Id;
            });
            
            drink.Name = NameEntry.Text;
            drink.Strength = Convert.ToInt32(StrengthEntry.Text);
            drink.Container = ContainerEntry.Text;
            drink.Size = Convert.ToInt32(SizeEntry.Text);
            
            Console.WriteLine("remove");
            foreach (DrinkRecipes recipe in remove.ToList())
            {
                Console.WriteLine(recipe.Id);
                drink.Ingredients.Remove(recipe);
            }
            
            Console.WriteLine("new");
            foreach (Widget widget in add)
            {
                Ingredient ingredient = widget.Data["object"] as Ingredient;
                int count = widget.Data["count"] is int ? (int) widget.Data["count"] : 0;
                DrinkRecipes recipes = new DrinkRecipes {Ingredient = ingredient, Count = count};
                Console.WriteLine(ingredient.Id);
                drink.Ingredients.Add(recipes);
            }


            
            return true;
        }

        public void SetDrink(Drink drink)
        {
            Drink = drink;

            NameEntry.Text = drink.Name;
            StrengthEntry.Text = drink.Strength.ToString();
            ContainerEntry.Text = drink.Container;
            SizeEntry.Text = drink.Size.ToString();
            foreach (Widget widget in IngredientList.Children)
            {
                IngredientList.Remove(widget);
            }
            foreach (DrinkRecipes recipe in drink.Ingredients)
            {
                ListBoxRow row = new ListBoxRow();
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
                    
                Label name = new Label(recipe.Ingredient.Name);
                Label size = new Label(recipe.Count.ToString());
                box.Add(name);
                box.Add(size);

                row.Data["object"] = recipe.Ingredient;
                row.Data["count"] = Convert.ToInt32(recipe.Count);
                row.Add(box);
                IngredientList.Add(row);
            }
            IngredientList.ShowAll();
        }
    }
}