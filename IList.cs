using Gtk;

namespace lab2
{
    public interface IList
    {
        void FillList();

        bool ScrollUp();
        bool ScrollDown();

        Widget[] GetChildren();

        void SelectRow(Widget row);

        IRow getSelected();

        void CancelSearch();
    }
}