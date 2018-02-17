using System.Windows.Input;

namespace MediaCenter.MVVM
{
    public interface ICommandEx : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
