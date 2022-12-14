using System;
using System.Text.RegularExpressions;
using Gtk;

namespace lab2
{
    public sealed class Form
    {
        public readonly Builder Builder;

        private Form()
        {
            Builder = new Builder("main.glade");
        }  
        private static Form instance = null;  
        public static Form Instance {  
            get {  
                if (instance == null) {  
                    instance = new Form();  
                }  
                return instance;  
            }  
        } 
        
        public static void OnlyInt(object sender, TextInsertedArgs e)
        {
            Entry entry = sender as Entry;
            if (entry == null)
            {
                return;
            }
            var isNumeric = int.TryParse(e.NewText, out _);
            if (!isNumeric)
            {
                
                entry.Text = entry.Text.Remove(entry.Text.Length - e.NewText.Length);
            }
        }
        
        public static void OnlyBigInt(object sender, TextInsertedArgs e)
        {
            Entry entry = sender as Entry;
            if (entry == null)
            {
                return;
            }

            bool isNumeric = true;

            foreach (char c in e.NewText)
            {
                isNumeric &= char.IsDigit(c);
            }
            
            if (!isNumeric)
            {
                
                entry.Text = entry.Text.Remove(entry.Text.Length - e.NewText.Length);
            }
        }
        
        public static void OnlyFloat(object sender, TextInsertedArgs e)
        {
            Entry entry = sender as Entry;
            if (entry == null)
            {
                return;
            }

            var regex = @"^\d+([\.]\d{0,2})?$"; 
            
            // var isNumeric = float.TryParse(e.NewText, out _);
            if (!Regex.Match(entry.Text,regex).Success)
            {
                entry.Text = entry.Text.Remove(entry.Text.Length - e.NewText.Length);
            }
        }
    }
}