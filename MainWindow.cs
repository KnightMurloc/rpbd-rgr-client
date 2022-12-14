using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace lab2
{
    class MainWindow : Window
    {
#pragma warning disable 649
        [UI] private Notebook TabsContainer;
#pragma warning restore 649
        [UI] private Box RightBox;
        [UI] private Box InfoBox;

        private TabManager TabManager;
        public MainWindow() : this(Form.Instance.Builder) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            
            TabManager = new TabManager(TabsContainer);
            
            TabsContainer.ShowAll();
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}
