using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using AimAssist.Core.Interfaces;
using Common.UI.Commands;

namespace AimAssist.Units.Implementation.Apps;

[AutoDataTemplate(typeof(AimAssistCommandsUnit))]
public partial class AimAssistCommandsControl : UserControl
{
    private readonly ICommandService commandService;

    public AimAssistCommandsControl(ICommandService commandService)
    {
        this.commandService = commandService;
        InitializeComponent();
        CreateCommandButtons();
    }

    private void CreateCommandButtons()
    {
        var commandProperties = commandService.GetKeymap()
            .ToList();

        foreach (var property in commandProperties)
        {
            if (commandService.TryGetCommand(property.Key, out var command))
            {
                if (command.CanExecute(this))
                {
                    var stackPanel = new StackPanel(){Orientation = Orientation.Horizontal};
                    var button = new Button
                    {
                        Content = property.Key,
                        Margin = new Thickness(0, 0, 0, 5),
                        Padding = new Thickness(10, 5, 10, 5),
                        Tag = command
                    };
                    
                    stackPanel.Children.Add(button);

                    if (property.Value != null)
                    {
                        var textBlock  = new TextBlock(){Text = property.Value.ToString()};
                        textBlock.Margin = new Thickness(20, 0, 0, 0);
                        stackPanel.Children.Add(textBlock);
                    }
                    
                    button.Click += CommandButton_Click;
                    CommandButtonsPanel.Children.Add(stackPanel);
                }
            }
        }
    }

    private void CommandButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is RelayCommand command)
        {
            command.Execute(this);
        }
    }
}
