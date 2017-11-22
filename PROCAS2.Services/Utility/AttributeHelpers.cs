using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;


namespace PROCAS2.Services.Utility
{
    public static class AttributeHelpers
    {

        public static Int32 GetMaxLength<T>(Expression<Func<T, string>> propertyExpression)
        {
            return GetPropertyAttributeValue<T, string, MaxLengthAttribute, Int32>(propertyExpression, attr => attr.Length);
        }

  

        //Required generic method to get any property attribute from any class
        public static TValue GetPropertyAttributeValue<T, TOut, TAttribute, TValue>(Expression<Func<T, TOut>> propertyExpression, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)expression.Member;
            var attr = propertyInfo.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;

            if (attr == null)
            {
                throw new MissingMemberException(typeof(T).Name + "." + propertyInfo.Name, typeof(TAttribute).Name);
            }

            return valueSelector(attr);
        }

    }
}
