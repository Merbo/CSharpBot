﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSharpBot
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Data;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Linq.Expressions;
    using System.ComponentModel;
    using System;


    public partial class BotOpDB : System.Data.Linq.DataContext
    {

        private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();

        #region Extensibility Method Definitions
        partial void OnCreated();
        partial void InsertNicks(Nicks instance);
        partial void UpdateNicks(Nicks instance);
        partial void DeleteNicks(Nicks instance);
        #endregion

        public BotOpDB(string connection) :
            base(connection, mappingSource)
        {
            if (!Environment.OSVersion.Platform.ToString().ToLower().Contains("win")) // Not windows?
                throw new NotSupportedException("Database is not supported on Linux.");
            OnCreated();
        }

        public BotOpDB(System.Data.IDbConnection connection) :
            base(connection, mappingSource)
        {
            OnCreated();
        }

        public BotOpDB(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) :
            base(connection, mappingSource)
        {
            OnCreated();
        }

        public BotOpDB(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) :
            base(connection, mappingSource)
        {
            OnCreated();
        }

        public System.Data.Linq.Table<Nicks> Nicks
        {
            get
            {
                return this.GetTable<Nicks>();
            }
        }
    }

    [global::System.Data.Linq.Mapping.TableAttribute()]
    public partial class Nicks : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

        private string _Nick;

        private System.Nullable<int> _AccessLevel;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        partial void OnCreated();
        partial void OnNickChanging(string value);
        partial void OnNickChanged();
        partial void OnAccessLevelChanging(System.Nullable<int> value);
        partial void OnAccessLevelChanged();
        #endregion

        public Nicks()
        {
            OnCreated();
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Name = "nick", Storage = "_Nick", DbType = "NVarChar(100) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public string Nick
        {
            get
            {
                return this._Nick;
            }
            set
            {
                if ((this._Nick != value))
                {
                    this.OnNickChanging(value);
                    this.SendPropertyChanging();
                    this._Nick = value;
                    this.SendPropertyChanged("Nick");
                    this.OnNickChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Name = "Access Level", Storage = "_AccessLevel", DbType = "Int")]
        public System.Nullable<int> AccessLevel
        {
            get
            {
                return this._AccessLevel;
            }
            set
            {
                if ((this._AccessLevel != value))
                {
                    this.OnAccessLevelChanging(value);
                    this.SendPropertyChanging();
                    this._AccessLevel = value;
                    this.SendPropertyChanged("AccessLevel");
                    this.OnAccessLevelChanged();
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, emptyChangingEventArgs);
            }
        }

        protected virtual void SendPropertyChanged(String propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
#pragma warning restore 1591
