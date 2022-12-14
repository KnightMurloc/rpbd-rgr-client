using Gtk;

namespace lab2
{
    public class TabManager
    {
        private Notebook TabContainer;

        private Tab[] Tabs = new Tab[10];
        
        public TabManager(Notebook tabContainer)
        {
            TabContainer = tabContainer;
            
            Tabs[0] = new EmployeesTab(this);
            Tabs[1] = new OrderTab(this);
            Tabs[2] = new IngredientTab(this);
            Tabs[3] = new ProviderTab(this);
            Tabs[4] = new ProductTab(this);
            Tabs[5] = new SnackTab(this);
            Tabs[6] = new DrinkTab(this);
            Tabs[7] = new DrinkOrderTab(this);
            Tabs[8] = new SnackOrderTab(this);
            Tabs[9] = new BankDetailTab(this);

            TabContainer.AppendPage(Tabs[0], new Label("сотрудники"));
            TabContainer.AppendPage(Tabs[1], new Label("приказы"));
            TabContainer.AppendPage(Tabs[2], new Label("ингредиенты"));
            TabContainer.AppendPage(Tabs[3], new Label("поставщики"));
            TabContainer.AppendPage(Tabs[4], new Label("продукты"));
            TabContainer.AppendPage(Tabs[5], new Label("закуски"));
            TabContainer.AppendPage(Tabs[6], new Label("напитки"));
            TabContainer.AppendPage(Tabs[7], new Label("заказы\nнапитков"));
            TabContainer.AppendPage(Tabs[8], new Label("заказы\nзакусок"));
            TabContainer.AppendPage(Tabs[9], new Label("рекизиты"));
        }

        public void SelectOnTabById(TabName name, int id)
        {
            int tabId = (int) name;
            Tabs[tabId].SelectById(id);
            TabContainer.CurrentPage = tabId;
        }

        public object SelectDialog(TabName name)
        {
            int tabId = (int) name;
            return Tabs[tabId].SelectDialog();
        }

        public Tab GetTab(TabName name)
        {
            int tabId = (int) name;
            return Tabs[tabId];
        }
    }

    public enum TabName {
        Employees = 0,
        Order = 1,
        Ingredients = 2,
        Provider = 3,
        Product = 4,
        Snack = 5,
        Drink = 6,
    };
}