using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace PreStorm
{
    /// <summary>
    /// Represents the base class for objects to which attributes can be mapped.
    /// </summary>
    public abstract class Feature : INotifyPropertyChanged
    {
        internal ServiceArgs ServiceArgs;
        internal Layer Layer;

        internal readonly Dictionary<string, object> UnmappedFields = new Dictionary<string, object>();
        internal readonly List<string> ChangedFields = new List<string>();
        internal bool GeometryChanged;

        private readonly Dictionary<string, string> _propertyToField;
        private readonly Dictionary<string, string> _fieldToProperty;

        /// <summary>
        /// Initializes a new instance of the Feature class.
        /// </summary>
        protected Feature()
        {
            OID = -1;
            _propertyToField = GetType().GetMappings().ToDictionary(m => m.Property.Name, m => m.FieldName);
            _fieldToProperty = GetType().GetMappings().ToDictionary(m => m.FieldName, m => m.Property.Name);
        }

        /// <summary>
        /// Instantiates a new object of the specified type.  Use this method instead of the constructor to ensure that the mapped properties automatically raise the PropertyChanged event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>() where T : Feature
        {
            return Proxy.Create<T>();
        }

        /// <summary>
        /// Returns the mapped properties.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Mapped[] GetMappings(Type type)
        {
            return type.GetMappings();
        }

        /// <summary>
        /// The Object ID of the feature.
        /// </summary>
        public int OID { get; internal set; }

        /// <summary>
        /// Indicates if this instance is bound to an actual row in the underlying table.
        /// </summary>
        [JsonIgnore]
        public bool IsDataBound => OID > -1;

        /// <summary>
        /// Returns the field names.  This includes unmapped fields.
        /// </summary>
        public string[] GetFieldNames()
        {
            return _fieldToProperty.Keys.Concat(UnmappedFields.Keys).ToArray();
        }

        private object GetValue(string fieldName)
        {
            if (_fieldToProperty.ContainsKey(fieldName))
            {
                return GetType().GetProperty(_fieldToProperty[fieldName]).GetValue(this, null);
            }

            if (UnmappedFields.ContainsKey(fieldName))
            {
                return UnmappedFields[fieldName];
            }

            throw new MissingFieldException($"Field '{fieldName}' does not exist.");
        }

        private void SetValue(string fieldName, object value)
        {
            if (_fieldToProperty.ContainsKey(fieldName))
            {
                var p = GetType().GetProperty(_fieldToProperty[fieldName]);

                if (!Equals(p.GetValue(this, null), value))
                {
                    p.SetValue(this, value, null);
                    IsDirty = true;
                    ChangedFields.Add(fieldName);
                }

                return;
            }

            if (UnmappedFields.ContainsKey(fieldName))
            {
                if (!Equals(UnmappedFields[fieldName], value))
                {
                    UnmappedFields[fieldName] = value;
                    IsDirty = true;
                    ChangedFields.Add(fieldName);
                }

                return;
            }

            UnmappedFields.Add(fieldName, value);
            IsDirty = true;
            ChangedFields.Add(fieldName);
        }

        /// <summary>
        /// Gets or sets a field value based on the field name.  This allows for manipulating fields that are not mapped to a property.  If the field is mapped to a property, the property value is accessed via Reflection.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object this[string fieldName]
        {
            get { return GetValue(fieldName); }
            set { SetValue(fieldName, value); }
        }

        private bool _isDirty;

        /// <summary>
        /// Indicates if any of the mapped properties has been changed via the property setter.
        /// </summary>
        [JsonIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            internal set
            {
                if (_isDirty == value)
                    return;

                _isDirty = value;

                if (!_isDirty)
                {
                    ChangedFields.Clear();
                    GeometryChanged = false;
                }

                RaisePropertyChanged(() => IsDirty);
            }
        }

        /// <summary>
        /// Indicates if the value of the specified property has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool Changed(string propertyName)
        {
            if (propertyName == "Geometry")
                return GeometryChanged;

            return ChangedFields.Contains(_propertyToField[propertyName]);
        }

        /// <summary>
        /// Represents the method that will handle the PropertyChanged event raised when a property is changed on a component.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called from a property setter to notify the framework that a member has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "IsDirty")
                return;

            if (_propertyToField.ContainsKey(propertyName))
            {
                ChangedFields.Add(_propertyToField[propertyName]);
                IsDirty = true;
            }
            else if (propertyName == "Geometry")
            {
                GeometryChanged = true;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Called from a property setter to notify the framework that a member has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertySelector"></param>
        public void RaisePropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;

            if (memberExpression != null)
                RaisePropertyChanged(memberExpression.Member.Name);
        }
    }

    /// <summary>
    /// Represents the base class for objects to which attributes can be mapped.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Feature<T> : Feature where T : Geometry
    {
        private T _geometry;

        /// <summary>
        /// The geometry of the underlying graphic object.
        /// </summary>
        public T Geometry
        {
            get { return _geometry; }
            set
            {
                _geometry = value;
                RaisePropertyChanged(nameof(Geometry));
            }
        }
    }

    /// <summary>
    /// Represents a feature whose attributes and geometry are dynamically accessed at runtime.
    /// </summary>
    public class DynamicFeature : Feature<Geometry>
    {
    }
}
