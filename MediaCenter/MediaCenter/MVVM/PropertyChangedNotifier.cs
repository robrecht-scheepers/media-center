using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.MVVM
{
    public abstract class Observable : INotifyPropertyChanged
    {
        private void InternalRaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            InternalRaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanges(params string[] properties)
        {
            foreach (var property in properties)
            {
                RaisePropertyChanged(property);
            }
        }

        protected void SetValue<T>(ref T refValue, T newValue, Action action = null, [CallerMemberName] string propertyName = null)
        {
            if (Equals(refValue, newValue)) return;
            
            refValue = newValue;

            InternalRaisePropertyChanged(propertyName);

            action?.Invoke();
        }

        private static string GetName<T>(Expression<Func<T>> prop)
        {
            var memberExpression = prop.Body as MemberExpression;
            if (memberExpression != null)
            {
                return memberExpression.Member.Name;
            }

            var unaryExpression = prop.Body as UnaryExpression;
            if (unaryExpression == null) throw new ArgumentException("Invalid property");

            var expression = unaryExpression.Operand as MemberExpression;
            if (expression == null) throw new ArgumentException("Invalid property");

            return expression.Member.Name;
        }
        
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
