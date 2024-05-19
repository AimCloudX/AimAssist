using System;
using System.Windows;

namespace AimPicker.Domain
{
    public class PickerSnippet : ISnippet
    {
        public PickerSnippet(string name, string text)
        {
            Name = name;
            Snippet = text;
        }

        public string Name { get; }

        public string Snippet { get; }

        public string Description => GetSnippetText;

        public string GetSnippetText => this.Snippet;

        public Func<UIElement> Factory =>() =>  new System.Windows.Controls.TextBox() { Text = this.Snippet }; 
    }

    public class PickerCommand : ICombo
    {
        public string Name { get; }

        public PickerCommand(string name, string description, Func<UIElement> factory)
        {
            Name = name;
            Description = description;
            Factory = factory;
        }

        public string Description { get; }
        public Func<UIElement> Factory { get; }
    }

    public interface ISnippet : ICombo
    {
        string Snippet { get; }
    }

    public interface ICombo
    {
        string Name { get; }

        string Description { get; }

        Func<UIElement> Factory { get; }

    }
}
