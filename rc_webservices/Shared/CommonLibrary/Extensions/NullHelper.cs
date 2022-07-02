using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CommonLibrary.Extensions
{
    public static class NullHelper
    {
        private class IsNullVisitor : ExpressionVisitor
        {
            public bool IsNull { get; private set; }
            public object CurrentObject { get; set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                base.VisitMember(node);
                if (CheckNull())
                {
                    return node;
                }

                var member = (PropertyInfo)node.Member;
                CurrentObject = member.GetValue(CurrentObject, null);
                CheckNull();
                return node;
            }

            private bool CheckNull()
            {
                if (CurrentObject == null)
                {
                    IsNull = true;
                }
                return IsNull;
            }
        }

        /// <summary>
        /// checks if an object is null
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentObject"></param>
        /// <returns></returns>
        public static bool IsNull<T>(T currentObject)
        {
            if (currentObject == null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// checks if the root object is null and if it's not, also checks if a property defined in the getter expression is null
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootObject"></param>
        /// <param name="getter"></param>
        /// <returns></returns>
        public static bool IsNull<T>(T rootObject, Expression<Func<T, object>> getter)
        {
            var visitor = new IsNullVisitor();
            visitor.CurrentObject = rootObject;
            visitor.Visit(getter);
            return visitor.IsNull;
        }

        /// <summary>
        /// checks if any chained objects with this extension method defined is null.
        /// 
        /// <para />Returns null when any of the chained object is null, the last object of the chained condition otherwise
        /// 
        /// <para />Example of use: PEP_EntityModels.Backoffice.ApprovalFlow t = deathAllowanceVM.DeathAllowance.NullOrValue(x => x.PersonProcess).NullOrValue(x => x.approvalFlowStep).NullOrValue(x => x.approvalFlow)
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static TResult NullOrValue<T, TResult>(this T source, Func<T, TResult> property) where T : class
        {
            return source == null ? default(TResult) : property(source);
        }
    }
}
