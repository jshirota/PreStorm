﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace PreStorm
{
    /// <summary>
    /// Represents the base class for objects that attributes can be mapped to.  For spatial objects, use the generic version of this type specifying the geometry type.
    /// </summary>
    public abstract class Feature : INotifyPropertyChanged
    {
        internal string Url;
        internal Esri.Layer Layer;
        internal ICredentials Credentials;
        internal Token Token;

        private readonly Dictionary<string, string> _propertyToField;
        internal readonly List<string> ChangedFields = new List<string>();
        internal bool GeometryChanged;

        /// <summary>
        /// Initializes a new instance of the Feature class.
        /// </summary>
        protected Feature()
        {
            OID = -1;
            _propertyToField = GetType().GetMappings().ToDictionary(m => m.Property.Name, m => m.Mapped.FieldName);
        }

        /// <summary>
        /// The Object ID of the feature.
        /// </summary>
        public int OID { get; internal set; }

        /// <summary>
        /// Indicates if this instance is bound to an actual row in the underlying table.
        /// </summary>
        public bool IsDataBound
        {
            get { return OID > -1; }
        }

        private bool _isDirty;

        /// <summary>
        /// Indicates if any of the mapped properties has been changed via the property setter.
        /// </summary>
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

                RaisePropertyChanged("IsDirty");
            }
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName != "IsDirty")
            {
                IsDirty = true;

                if (_propertyToField.ContainsKey(propertyName))
                    ChangedFields.Add(_propertyToField[propertyName]);
            }
        }
    }

    /// <summary>
    /// Represents the base class for objects that attributes can be mapped to.  For non-spatial objects, use the non-generic version of this type.
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
                RaiseGeometryChanged();
            }
        }

        /// <summary>
        /// Flags the geometry to be updated.  Use this when editing by mutating the internal state of the geometry.
        /// </summary>
        public void RaiseGeometryChanged()
        {
            GeometryChanged = true;
            RaisePropertyChanged("Geometry");
        }
    }
}
