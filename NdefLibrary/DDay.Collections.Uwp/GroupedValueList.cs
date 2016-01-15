using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.Collections
{
    public class GroupedValueList<TGroup, TInterface, TItem, TValueType> :
        GroupedList<TGroup, TInterface>,
        IGroupedValueList<TGroup, TInterface, TItem, TValueType>
        where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
        where TItem : new()
    {
        #region Protected Methods

        virtual protected bool AddToExisting(TGroup group, IEnumerable<TValueType> values)
        {
            IEnumerable<TInterface> items = AllOf(group);
            if (items != null)
            {
                var container = items.FirstOrDefault();
                if (container != null)
                {
                    // Add a value to the first matching item in the list
                    container.SetValue(values);

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region IKeyedValueList<TGroup, TObject, TValueType> Members

        virtual public void Set(TGroup group, TValueType value)
        {
            var values = new TValueType[] { value };

            // If this object is a valid container, and we don't
            // already have a container for this group, then
            // let's reuse the existing container.
            if (value is TInterface)
            {
                var container = value as TInterface;

                // If we can't consolidate our value with another container,
                // or another container doesn't exist yet, then let's use
                // this as our container (and add it to the list)
                if (!ContainsKey(group))
                {
                    // Set the group on our container
                    container.Group = group;

                    // Add the container to the list
                    Add(container);
                    return;
                }
                else if (AddToExisting(group, values))
                {
                    // If we already have a container for this group, then 
                    // pass the value along to be added to the existing container.
                    return;
                }
            }
             
            // Otherwise, if not a valid container, pass along so a 
            // container can be generated.
            Set(group, values);
        }

        virtual public void Set(TGroup group, IEnumerable<TValueType> values)
        {
            // If a group already exists, and we can consolidate with it,
            // then let's do so.
            if (ContainsKey(group))
            {
                if (AddToExisting(group, values))
                    return;
            }

            // No matching container was found, add a new container to the list
            TInterface container = Activator.CreateInstance(typeof(TItem)) as TInterface;

            // Set the group for the container
            container.Group = group;

            // Add the container to the list
            Add(container);

            // Set the list of values for the container
            container.SetValue(values);
        }

        virtual public TType Get<TType>(TGroup group)
        {
            var firstItem = AllOf(group).FirstOrDefault();
            if (firstItem != null &&
                firstItem.Values != null)
            {
                return firstItem
                    .Values
                    .OfType<TType>()
                    .FirstOrDefault();
            }
            return default(TType);
        }

        virtual public IList<TType> GetMany<TType>(TGroup group)
        {
            return new GroupedValueListProxy<TGroup, TInterface, TItem, TValueType, TType>(this, group);
        }

        #endregion
    }
}
