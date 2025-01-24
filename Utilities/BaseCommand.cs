using System.Windows.Input;

namespace FeedFetcher.Utilities
{
    public class BaseCommand<T> : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private Action<T> _execute;
        private Func<T,bool> _canExecute;
        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute((T)parameter);
            return true;
        }
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
        public BaseCommand(Action<T> execute)
        {
            this._execute = execute;
        }
        public BaseCommand(Action<T> execute, Func<T, bool> canExecute) : this(execute)
        {
            _canExecute = canExecute;
        }
    }
}
