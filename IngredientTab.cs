using System;
using Gtk;
using lab2.Models;
using UI = Gtk.Builder.ObjectAttribute;
using Unit = lab2.Models.Unit;
using static lab2.locale.NGettextShortSyntax;


namespace lab2
{
    public class IngredientTab : Tab
    {
        class Row : ListBoxRow, IRow
        {
            private Ingredient _ingredient;

            public Ingredient Ingredient
            {
                get
                {
                    Repository.Instance.Refresh(ref _ingredient);
                    return _ingredient;
                }

                set => _ingredient = value;
            }

            public Label NameLabel { get; private set; }
            public Label UnitLabel { get; private set; }

            public void Init(object obj)
            {
                Ingredient ingredient = obj as Ingredient;
                if (ingredient == null)
                {
                    return;
                }
                _ingredient = ingredient;
                
                Box box = new Box(Orientation.Horizontal,0);
                box.Homogeneous = true;
               
                NameLabel = new Label(ingredient.Name);
                UnitLabel = new Label(ingredient.Unit.Name);
                
                box.Add(NameLabel);
                box.Add(UnitLabel);

                Add(box);
            }

            public int GetId()
            {
                return _ingredient.Id;
            }

            public IEntity GetEntity()
            {
                return Ingredient;
            }
        }

        private readonly EntityList<Ingredient, Row> List;
        private readonly IngredientMenu Menu = new IngredientMenu();

        public IngredientTab(TabManager tabManager) : base(tabManager)
        {
            List = (EntityList<Ingredient, Row>) CreateList();
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
            OnRemove?.Invoke(row.Ingredient);
            Repository.Instance.Delete(row.Ingredient);
            List.RemoveRow(row);
        }

        private void CreateEntity(object? sender, EventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton(_("Cancel"), ResponseType.Cancel);
            
            IngredientMenu menu = new IngredientMenu();

            ((Container) dialog.Children[0]).Add(menu);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    Ingredient ingredient = new Ingredient();
                    if (menu.SettingData(ingredient))
                    {
                        Repository.Instance.Create(ingredient);
                        List.AddEntity(ingredient);
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
                Repository.Instance.Update(Menu.Ingredient);
                Row row = List.GetRow();
                if (row != null && row.GetId() == Menu.Ingredient.Id)
                {
                    Ingredient ingredient = Menu.Ingredient;
                    row.NameLabel.Text = ingredient.Name;
                    row.UnitLabel.Text = ingredient.Unit.Name;
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
            if (box.Children.Length == 0 || box.Children[0].Name != "ingrident_menu")
            {
                if (box.Children.Length != 0)
                {
                    box.Remove(box.Children[0]);
                }
                box.Add(Menu);
            }
            
            Menu.SetIngredient(row.Ingredient);
            box.ShowAll();
        }

        protected override IList CreateList()
        {
            var list = new EntityList<Ingredient,Row>();
            list.AddColumnTitle(_("Name"));
            list.AddColumnTitle(_("Unit"));
            return list;
        }

        protected override void Search(string input)
        {
            // string hql = $"lower(e.Name) like '%{input}%' or " +
            //              $"lower(e.Unit.Name) like '%{input}%'";
            string cond = $"name ilike '%{input}%'";
            List.SetSearchCondition(cond);
        }
    }

    public class IngredientMenu : Box
    {
        public Ingredient Ingredient { get; set; }

        [UI] public readonly Entry NameEntry;
        [UI] public readonly ComboBoxText UnitCombo;
        
        public IngredientMenu() : this(new Builder("ingridient_menu.glade")){}

        private IngredientMenu(Builder builder) : base(builder.GetRawOwnedObject("box"))
        {
            builder.Autoconnect(this);

            var units = Repository.Instance.FindByCondition<Unit>(new Query(){Table = "Unit"});
            foreach (var unit in units)
            {
                UnitCombo.Append(unit.Name,_(unit.Name));
            }
        }

        private void ShowError(string message)
        {
            MessageDialog dialog = new MessageDialog(null,DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,false,null);
            dialog.Text = message;
            dialog.Run();
            dialog.Destroy();
        }
        
        public void SetIngredient(Ingredient ingredient)
        {
            Ingredient = ingredient;

            NameEntry.Text = ingredient.Name;
            UnitCombo.ActiveId = ingredient.Unit.Name;
        }

        public bool SettingData()
        {
            return SettingData(Ingredient);
        }
        
        public bool SettingData(Ingredient ingredient)
        {
            if (string.IsNullOrEmpty(NameEntry.Text))
            {
                ShowError(_("no name specified"));
                return false;
            }

            Unit unit = null;
            // var postQuery = Repository.Instance.FindByCondition<Unit>($"from Unit u where u.Name = '{UnitCombo.ActiveId}'", 1);
            {
                Query query = new Query(){Table = "Unit", Count = 1, Condition = $"name = '{UnitCombo.ActiveId}'"};
                var result = Repository.Instance.FindByCondition<Unit>(query);

                if (result.Count != 0)
                {
                    unit = result[0];
                }
            }
            
            if (unit == null)
            {
                ShowError(_("this unit doesn't exist"));
                return false;
            }

            ingredient.Name = NameEntry.Text;
            ingredient.Unit = unit;
            
            return true;
        }
    }
}