using System.Windows.Input;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>A command that runs an action.</summary>
internal sealed class RelayCommand(Action action) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => action();
}
