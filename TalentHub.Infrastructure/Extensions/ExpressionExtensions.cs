using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace TalentHub.Infrastructure.Extensions
{

    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));// Create a new parameter of type T

            var firstBody = new ReplaceExpressionVisitor(first.Parameters[0], parameter).Visit(first.Body);

            var secondBody = new ReplaceExpressionVisitor(second.Parameters[0], parameter).Visit(second.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(firstBody!, secondBody!), parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression? Visit(Expression? node)
            {
                return node == _oldValue ? _newValue : base.Visit(node);
            }
        }
    }
}

