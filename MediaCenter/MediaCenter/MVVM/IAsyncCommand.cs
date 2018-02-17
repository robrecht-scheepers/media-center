using System.Threading.Tasks;

namespace MediaCenter.MVVM
{
    public interface IAsyncCommand : ICommandEx
    {
        Task ExecuteAsync(object parameter);
    }
}
