using System;
using Gtk;
using lab2.Models;

namespace lab2
{
    public class EntityList<T, E> : Box, IList where E : Widget, IRow, new() where T : IEntity
    {
        private const int PACAGE_SIZE = 30;
    
        private Box Box;
        private ScrolledWindow Scroll;
        public ListBox List { get; private set; }
        
        private EntityIterator<T> Iterator;
        
        private readonly Box Header = new Box(Orientation.Horizontal,0);
        
        public EntityList() : base(Orientation.Vertical, 0)
        {
            Iterator = new EntityIterator<T>(PACAGE_SIZE);
            Box = new Box(Orientation.Vertical,0);
            
            Scroll = new ScrolledWindow();
            Scroll.Vexpand = true;
            
            Box scrollBox = new Box(Orientation.Vertical,0);
            
            List = new ListBox();
            List.Vexpand = true;

            Header.Hexpand = true;
            Header.Homogeneous = true;

            scrollBox.Add(Header);
            scrollBox.Add(List);

            Scroll.Add(scrollBox);
            
            Box.Add(Scroll);
            
            FillList();
            
            this.Add(Box);
            
            Scroll.EdgeReached += ScrollEvent;
        }

        private new void ScrollEvent(object sender, EdgeReachedArgs e)
        {
            if (e.P0 == PositionType.Bottom)
            {
                ScrollDown();
            }else if (e.P0 == PositionType.Top)
            {
                ScrollUp();
            }
        }

        public bool ScrollDown()
        {
            var newList = Iterator.NextPackage();
            if (newList.Count == 0)
            {
                return false;
            }

            
            foreach (T entity in newList)
            {
                var row = new E();
                row.Init(entity);
                List.Add(row);
            }
            var rows = List.Children;
            if (rows.Length > PACAGE_SIZE) 
            {
                for (int i = 0; i < rows.Length - PACAGE_SIZE;i++)
                {
                    List.Remove(rows[i]);
                }
            }

            Scroll.Vadjustment.Value = 250;
            
            List.ShowAll();
            return true;
        }

        public Widget[] GetChildren()
        {
            return List.Children;
        }

        public void SelectRow(Widget row)
        {
            List.SelectRow(row as ListBoxRow);
        }

        public IRow getSelected()
        {
            return (IRow) List.SelectedRow;
        }

        public void CancelSearch()
        {
            Iterator.Condition = "";
        }

        public bool ScrollUp()
        {
            var newList = Iterator.PrevPackage();
            if (newList.Count == 0)
            {
                return false;
            }

            foreach (T entity in newList)
            {
                var row = new E();
                row.Init(entity);
                List.Insert(row,0);
            }
            var rows = List.Children;
            if (rows.Length > PACAGE_SIZE) 
            {
                for (int i = rows.Length - 1; i >= PACAGE_SIZE;i--)
                {
                    List.Remove(rows[i]);

                }
            }

            Scroll.Vadjustment.Value = 100;
            
            List.ShowAll();
            return true;
        }
        
        public void FillList()
        {
            Iterator.Reset();
            // Scroll.Vadjustment.Value = 0;
            foreach(Widget row in List.Children)
            {
                List.Remove(row);
            }
            
            var list = Iterator.NextPackage();
            foreach (T entity in list)
            {
                var row = new E();
                row.Init(entity);
                List.Add(row);
            }
            List.ShowAll();
        }

        public E GetRow()
        {
            return List.SelectedRow as E;
        }

        public void AddEntity(T entity)
        {
            // E row = new E();
            // row.Init(entity);
            // List.Add(row);
            // List.ShowAll();
            
            ScrollDown();
        }

        public void RemoveRow(E row)
        {
            List.Remove(row);
        }

        public void SetSearchCondition(string hql)
        {
            Iterator.Condition = hql;
        }

        public void AddColumnTitle(string title)
        {
            Header.Add(new Label(title));
        }
    }

    public interface IRow
    {
        public void Init(object data);
        public int GetId();
        public IEntity GetEntity();
    }
}