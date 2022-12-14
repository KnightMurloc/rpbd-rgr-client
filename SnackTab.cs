using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using lab2.Models;
using static lab2.locale.NGettextShortSyntax;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    public class SnackTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private Snack _snack;

            public Snack Snack
            {
                get
                {
                    Repository.Instance.Refresh(ref _snack);
                    return _snack;
                }

                set => _snack = value;
            }

            public Label NameLabel { get; private set; }
            public Label SizeLabel { get; private set; }
            
            public void Init(object obj)
            {
                Snack snack = obj as Snack;
                if (snack == null)
                {
                    return;
                }

                _snack = snack;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                NameLabel = new Label(snack.Name);
                SizeLabel = new Label(snack.Size.ToString());
                
                box.Add(NameLabel);
                box.Add(SizeLabel);
                
                Add(box);
            }

            public int GetId()
            {
                return Snack.Id;
            }

            public IEntity GetEntity()
            {
                return Snack;
            }
        }

        private readonly EntityList<Snack, Row> List;
        private readonly SnackMenu Menu = new SnackMenu();
        
        public SnackTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<Snack, Row>) CreateList();
            SetList(List);
            
            List.List.RowSelected += Select;
            
            
            Button saveButton = new Button("gtk-save");
            saveButton.Clicked += SaveCurrent;
            Menu.Add(saveButton);
            Menu.ShowAll();
            
            AddButton.Clicked += CreateEntity;
            RemoveButton.Clicked += RemoveEntity;
            
            Menu.AddButton.Clicked += AddInredient;
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

            OnRemove.Invoke(row.Snack);
            Repository.Instance.Delete(row.Snack);
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


        private void SaveCurrent(object? sender, EventArgs e)
        {
            if (Menu.SettingData())
            {
                Repository.Instance.Update(Menu.Snack);
                Row row = List.GetRow();
                if (row != null && row.GetId() == Menu.Snack.Id)
                {
                    Snack snack = Menu.Snack;
                    row.NameLabel.Text = snack.Name;
                    row.SizeLabel.Text = snack.Size.ToString();
                }
            }
        }

        private void AddInredient(object? sender, EventArgs e)
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

        private void Select(object o, RowSelectedArgs args)
        {
            Row row = args.Row as Row;
            if (row == null)
            {
                return;
            }

            Box box = (Box) Form.Instance.Builder.GetObject("InfoBox");
            if (box.Children.Length == 0 || box.Children[0].Name != "snack_info")
            {
                if (box.Children.Length != 0)
                {
                    box.Remove(box.Children[0]);
                }
                box.Add(Menu);
            }
            
            Menu.SetSnack(row.Snack);
            box.ShowAll();
        }

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            SnackMenu menu = new SnackMenu();

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
                    Snack snack = new Snack();
                    if (menu.SettingData(snack))
                    {
                        Repository.Instance.Create(snack);
                        List.AddEntity(snack);
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
            var list = new EntityList<Snack,Row>();
            list.AddColumnTitle(_("Name"));
            list.AddColumnTitle(_("Size"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Name) like '%{input}%' or " +
            //              (int.TryParse(input, out var _) ? $"e.Size = {input}" : "");
            string hql = (int.TryParse(input, out var _) ? $"size = {input}" : "") + 
                         $"name ilike '%{input}%'";
            List.SetSearchCondition(hql);
        }
    }

    class SnackMenu : Box
    {
        public Snack Snack { get; set; }

#pragma warning disable 649
        [UI] public readonly Entry NameEntry;
        [UI] public readonly Entry SizeEntry;
        [UI] public readonly Button AddButton;
        [UI] public readonly Button RemoveButton;
        [UI] public readonly ListBox IngredientList;
#pragma warning restore 649

        public SnackMenu() : this(new Builder("snack_menu.glade")){}

        private SnackMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);
            this.Expand = true;
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
            return SettingData(Snack);
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
        
        public bool SettingData(Snack snack)
        {
            if (string.IsNullOrEmpty(NameEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }

            if (string.IsNullOrEmpty(SizeEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }
            
            var newList = IngredientList.Children;
            var oldList = snack.Ingredients;

            var add = different(newList.ToList(), oldList, (recipes, widget) => recipes.Ingredient.Id == ((Ingredient) widget.Data["object"]).Id);
            var remove = different(oldList, newList.ToList(), (widget, recipes) =>
            {
                return recipes.Ingredient.Id == ((Ingredient) widget.Data["object"]).Id;
            });
            // var add = newList.Where(p => !oldList.Any(l => (p.Data["object"] as Ingredient).Id == l.Id));
            // var remove = oldList.Where(p => !newList.Any(l => p.Id == (l.Data["object"] as Ingredient).Id));

            snack.Name = NameEntry.Text;
            snack.Size = Convert.ToInt32(SizeEntry.Text);
            
            Console.WriteLine("remove");
            foreach (SnackRecipes recipe in remove)
            {
                Console.WriteLine(recipe.Id);
                snack.Ingredients.Remove(recipe);
                // Repository.Instance.Delete(recipe);
            }
            
            Console.WriteLine("new");
            foreach (Widget widget in add)
            {
                Ingredient ingredient = widget.Data["object"] as Ingredient;
                int count = widget.Data["count"] is int ? (int) widget.Data["count"] : 0;
                SnackRecipes recipes = new SnackRecipes {Ingredient = ingredient, Count = count};
                Console.WriteLine(ingredient.Id);
                snack.Ingredients.Add(recipes);
            }
            
            
            
            return true;
        }

        public void SetSnack(Snack snack)
        {
            Snack = snack;

            NameEntry.Text = snack.Name;
            SizeEntry.Text = snack.Size.ToString();
            foreach (Widget widget in IngredientList.Children)
            {
                IngredientList.Remove(widget);
            }
            foreach (SnackRecipes recipe in snack.Ingredients)
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