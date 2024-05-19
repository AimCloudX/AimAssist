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

        public IPreviewFactory Factory => new SnippetPreviewFactory(); 
        public UIElement Create()
        {
            return this.Factory.Create(this);
        }
    }

    public interface IPreviewFactory
    {
        UIElement Create(ICombo combo);
    }

    public class SnippetPreviewFactory : IPreviewFactory
    {
        public UIElement Create(ICombo combo)
        {
            return new System.Windows.Controls.TextBox() { Text = combo.Description };
        }
    }

    public class PickerCommand : ICombo
    {
        public string Name { get; }

        public PickerCommand(string name, string description, IPreviewFactory factory)
        {
            Name = name;
            Description = description;
            Factory = factory;
        }

        public string Description { get; }
        public IPreviewFactory Factory { get; }

        public UIElement Create()
        {
            return this.Factory.Create(this);
        }
    }

    public interface ISnippet : ICombo
    {
        string Snippet { get; }
    }

    public interface ICombo
    {
        string Name { get; }

        string Description { get; }

        UIElement Create();
    }

}
